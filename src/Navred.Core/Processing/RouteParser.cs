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

        public async Task<IEnumerable<Leg>> ParseRouteAsync(
            Route route, RouteOptions stopTimeOptions = RouteOptions.None)
        {
            var preprocessedRoute = await this.PreprocessRouteAsync(route, stopTimeOptions);
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

        private async Task<Route> PreprocessRouteAsync(Route route, RouteOptions options)
        {
            Validator.ThrowIfNull(route, "Empty route.");

            var optionsInvalid = 
                options & RouteOptions.EstimateDuplicates & RouteOptions.RemoveDuplicates;

            if (optionsInvalid > 0)
            {
                throw new InvalidOperationException("Invalid options.");
            }

            var copy = route.Copy();
            copy.Stops = copy.Stops.Select(s =>
            {
                s.Name = string.IsNullOrWhiteSpace(s.Name) ? "EMPTY" : s.Name;

                return s;
            }).ToList();
            copy.Stops = new HashSet<Stop>(route.Stops).ToList();
            copy.Stops = this.placesManager.DeducePlacesFromStops(
                this.cultureProvider.Name, copy.Stops, false).ToList();
            copy.Stops = copy.Stops.Where(s => !s.Place.IsNull()).ToList();
            var toRemove = new List<Stop>();

            foreach (var (current, next) in copy.Stops.AsPairs())
            {
                var departure = DateTime.Now.Date + current.Time.Time;
                var arrival = DateTime.Now.Date + next.Time.Time;

                if (departure > arrival)
                {
                    arrival = await this.estimator.EstimateArrivalTimeAsync(
                        current.Place, next.Place, departure, route.Mode);
                    next.Time = arrival.TimeOfDay;
                }

                if (departure.Equals(arrival) && options.Matches(RouteOptions.RemoveDuplicates))
                {
                    toRemove.Add(current);
                }
                else if (departure.Equals(arrival) && 
                    options.Matches(RouteOptions.EstimateDuplicates))
                {
                    arrival = await this.estimator.EstimateArrivalTimeAsync(
                        current.Place, next.Place, departure, route.Mode);
                    next.Time = arrival.TimeOfDay;
                }
            }

            copy.Stops = copy.Stops.Except(toRemove).ToList();

            return copy;
        }
    }
}
