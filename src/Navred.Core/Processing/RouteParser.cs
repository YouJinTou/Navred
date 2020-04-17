using Microsoft.Extensions.Logging;
using Navred.Core.Cultures;
using Navred.Core.Estimation;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Models;
using Navred.Core.Places;
using Navred.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Core.Processing
{
    public class RouteParser : IRouteParser
    {
        private readonly IPlacesManager placesManager;
        private readonly ITimeEstimator estimator;
        private readonly ICultureProvider cultureProvider;
        private readonly ILogger<RouteParser> logger;

        public RouteParser(
            IPlacesManager placesManager,
            ITimeEstimator estimator,
            ICultureProvider cultureProvider,
            ILogger<RouteParser> logger)
        {
            this.placesManager = placesManager;
            this.estimator = estimator;
            this.cultureProvider = cultureProvider;
            this.logger = logger;
        }

        public async Task<IEnumerable<Leg>> ParseRouteAsync(Route route)
        {
            var preprocessedRoute = await this.PreprocessRouteAsync(route);
            var schedule = new Schedule();

            foreach (var (current, next) in preprocessedRoute.Stops.AsPairs())
            {
                try
                {
                    var departureTimes = route.DaysOfWeek.GetValidUtcTimesAhead(
                        current.Time,
                        Constants.CrawlLookaheadDays,
                        this.cultureProvider.GetHolidays())
                        .ToList();
                    var arrivalTimes = route.DaysOfWeek.GetValidUtcTimesAhead(
                        next.Time,
                        Constants.CrawlLookaheadDays,
                        this.cultureProvider.GetHolidays())
                        .ToList();

                    for (int t = 0; t < departureTimes.Count; t++)
                    {
                        var leg = new Leg(
                            from: current.Place,
                            to: next.Place,
                            utcDeparture: departureTimes[t],
                            utcArrival: arrivalTimes[t],
                            carrier: route.Carrier,
                            mode: route.Mode,
                            info: route.Info,
                            price: this.cultureProvider.ParsePrice(next.Price),
                            fromSpecific: current.Address,
                            toSpecific: next.Address,
                            arrivalEstimated: next.Time.Estimated,
                            priceEstimated: false);

                        schedule.AddLeg(leg);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{ex.Message}: {string.Join(" | ", preprocessedRoute.Stops)}");
                    //this.logger.LogError(ex, current.ToString());
                }
            }

            var result = schedule.Permute();

            return result;
        }

        private async Task<Route> PreprocessRouteAsync(Route route)
        {
            Validator.ThrowIfNull(route, "Empty route.");

            var copy = route.Copy();
            copy = copy.TagEmptyStops();
            copy = copy.RemoveDuplicates();
            copy = copy.RemoveBanned();
            var deduced = this.placesManager.DeducePlacesFromStops(
                this.cultureProvider.Name, copy.Stops, false).ToList();
            copy = copy.SetStops(deduced);
            copy = copy.RemovePlaceless();
            copy = await this.FixTimelineAsync(copy);
            copy = copy.RemoveOffendingEstimables();

            if (!copy.IsValid)
            {
                throw new InvalidOperationException(
                    $"Unresolvable schedule: {string.Join(" | ", copy.Stops)}");
            }

            return copy;
        }
        
        private async Task<Route> FixTimelineAsync(Route route)
        {
            foreach (var (current, next) in route.Stops.AsPairs())
            {
                var departure = DateTime.Now.Date + current.Time.Time;
                var arrival = DateTime.Now.Date + next.Time.Time;

                if (current.Time.Equals(LegTime.Estimable))
                {
                    departure = await this.estimator.EstimateDepartureTimeAsync(
                        next.Place, current.Place, arrival, route.Mode);
                    current.Time = departure.TimeOfDay;
                    current.Time.Estimated = true;
                }

                if (next.Time.Equals(LegTime.Estimable))
                {
                    arrival = await this.estimator.EstimateArrivalTimeAsync(
                        current.Place, next.Place, departure, route.Mode);
                    next.Time = arrival.TimeOfDay;
                    next.Time.Estimated = true;
                }

                if (departure > arrival && 
                    !route.Estimables.Contains(current, new StopNameEqualityComparer()))
                {
                    arrival = await this.estimator.EstimateArrivalTimeAsync(
                        current.Place, next.Place, departure, route.Mode);
                    next.Time = arrival.TimeOfDay;
                    next.Time.Estimated = true;
                }

                if (departure.Equals(arrival))
                {
                    arrival = await this.estimator.EstimateArrivalTimeAsync(
                        current.Place, next.Place, departure, route.Mode);
                    next.Time = arrival.TimeOfDay;
                    next.Time.Estimated = true;
                }
            }

            return route;
        }
    }
}
