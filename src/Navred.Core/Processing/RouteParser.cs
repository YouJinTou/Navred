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

            this.TagEmptyStops(copy);

            this.RemoveDuplicates(copy);

            this.RemoveBanned(copy);

            this.DeducePlaces(copy);

            this.RemoveNotFound(copy);

            await this.FixTimelineAsync(copy);

            if (!copy.Stops.IsAscending(s => s.Time.Time))
            {
                throw new InvalidOperationException(
                    $"Unresolvable schedule: {string.Join(" | ", copy.Stops)}");
            }

            return copy;
        }

        private void RemoveBanned(Route route)
        {
            route.Stops = route.Stops.Except(route.Banned, new StopNameEqualityComparer()).ToList();
        }

        private void TagEmptyStops(Route route)
        {
            route.Stops = route.Stops.Select(s =>
            {
                s.Name = string.IsNullOrWhiteSpace(s.Name) ? "EMPTY" : s.Name;

                return s;
            }).ToList();
        }

        private void RemoveDuplicates(Route route)
        {
            var reversed = route.Stops.Reverse();
            reversed = new HashSet<Stop>(
                reversed, new StopTimeEqualityComparer()).ToList();
            var duplicatesWithEstimableTimes = reversed
                .GroupBy(s => s.CompositeName)
                .Where(g => g.Count() > 1)
                .SelectMany(g => g)
                .Where(s => s.Time.Equals(LegTime.Estimable))
                .ToList();
            reversed = reversed.Except(
                duplicatesWithEstimableTimes, new StopTimeEqualityComparer()).ToList();
            reversed = new HashSet<Stop>(reversed, new StopNameEqualityComparer()).ToList();
            route.Stops = reversed.Reverse().ToList();
        }

        private void DeducePlaces(Route route) =>
            route.Stops = this.placesManager.DeducePlacesFromStops(
                this.cultureProvider.Name, route.Stops, false).ToList();

        private void RemoveNotFound(Route route) =>
            route.Stops = route.Stops.Where(s => !s.Place.IsNull()).ToList();

        private async Task FixTimelineAsync(Route route)
        {
            var estimables = route.Stops
                .Where(s => s.Time.Equals(LegTime.Estimable))
                .Select(s => s.Copy())
                .ToList();

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
                    !estimables.Contains(current, new StopNameEqualityComparer()))
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
        }
    }
}
