using Microsoft.Extensions.Logging;
using Navred.Core.Cultures;
using Navred.Core.Estimation;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Models;
using Navred.Core.Places;
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
        private Route parsed;

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

        public Route Parsed => this.parsed;

        public async Task<IEnumerable<Leg>> ParseRouteAsync(Route route)
        {
            this.parsed = await this.PreprocessRouteAsync(route);
            var legs = new List<Leg>();
            var date = DateTime.UtcNow.AddDays(1).Date;

            for (int s = 0; s < this.parsed.Stops.Count; s++)
            {
                var current = this.parsed.Stops[s];

                for (int n = s + 1; n < this.parsed.Stops.Count; n++)
                {
                    var next = this.parsed.Stops[n];

                    try
                    {
                        var departureTimes = route.DaysOfWeek.GetValidUtcTimesAhead(
                            date + current.Time.Time,
                            Constants.CrawlLookaheadDays,
                            this.cultureProvider.GetHolidays())
                            .ToList();
                        date = (current.Time > next.Time) ?
                            departureTimes[0].Date.ReturnBigger(date).AddDays(1) :
                            departureTimes[0].Date.ReturnBigger(date);
                        var arrivalTimes = route.DaysOfWeek.GetValidUtcTimesAhead(
                            date + next.Time.Time,
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
                                price: route.GetPrice(current, next),
                                fromSpecific: current.Address,
                                toSpecific: next.Address,
                                departureEstimated: current.Time.Estimated,
                                arrivalEstimated: next.Time.Estimated,
                                priceEstimated: false);

                            legs.Add(leg);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.Message}: {string.Join(" | ", this.parsed.Stops)}");
                        //this.logger.LogError(ex, current.ToString());
                    }

                }
            }

            return legs;
        }

        private async Task<Route> PreprocessRouteAsync(Route route)
        {
            route.ThrowIfNull("Empty route.");

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
