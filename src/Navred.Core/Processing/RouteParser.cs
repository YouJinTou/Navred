using Microsoft.Extensions.Logging;
using Navred.Core.Estimation;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Places;
using Navred.Core.Tools;
using Navred.Crawling.Models;
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
        private readonly ILogger<RouteParser> logger;

        public RouteParser(
            IPlacesManager placesManager,
            ITimeEstimator estimator,
            ILogger<RouteParser> logger)
        {
            this.placesManager = placesManager;
            this.estimator = estimator;
            this.logger = logger;
        }

        public async Task<IEnumerable<Leg>> ParseRouteAsync(
            RouteData route, StopTimeOptions stopTimeOptions = StopTimeOptions.None)
        {
            var preprocessedRoute = this.PreprocessRoute(route);
            var stopInfos = this.GetStopInfos(route);
            var schedule = new Schedule();

            foreach (var (current, next) in stopInfos.AsPairs())
            {
                try
                {
                    var departureTimes = route.DaysOfWeek.GetValidUtcTimesAhead(
                        current.Time, Constants.CrawlLookaheadDays).ToList();
                    var arrivalTimes = route.DaysOfWeek.GetValidUtcTimesAhead(
                        next.Time, Constants.CrawlLookaheadDays).ToList();

                    for (int t = 0; t < departureTimes.Count; t++)
                    {
                        var enforceResult = await this.TryEnforceStopTimeOptionsAsync(
                            current.Stop, 
                            next.Stop, 
                            route.Mode, 
                            departureTimes[t], 
                            arrivalTimes[t], 
                            stopTimeOptions);

                        if (enforceResult.Item1)
                        {
                            var leg = new Leg(
                            from: current.Stop,
                            to: next.Stop,
                            utcDeparture: departureTimes[t],
                            utcArrival: enforceResult.Item2,
                            carrier: route.Carrier,
                            mode: route.Mode,
                            info: route.Info,
                            price: next.Price,
                            fromSpecific: current.DetailedName,
                            toSpecific: next.DetailedName,
                            arrivalEstimated: next.Time.Estimated,
                            priceEstimated: false);

                            schedule.AddLeg(leg);
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, current.ToString());
                }
            }

            var result = schedule.Permute();

            return result;
        }

        private IEnumerable<StopInfo> GetStopInfos(RouteData route)
        {
            var placesByKey = this.placesManager.DeducePlacesFromStops(
                route.Country, route.Stops, false);
            var stopInfos = route.Stops.Select((kvp, i) => new StopInfo
            {
                DetailedName = route.Addresses[i],
                Price = route.Prices[i],
                Stop = placesByKey[route.Stops[i]],
                Time = route.StopTimes[i]
            }).Where(s => s.Stop != null).ToList();

            return stopInfos;
        }

        private RouteData PreprocessRoute(RouteData route)
        {
            Validator.ThrowIfNull(route, "Empty route.");

            return this.RemoveDuplicateStops(route);
        }

        private RouteData RemoveDuplicateStops(RouteData route)
        {
            var seenStops = new HashSet<string>();
            var duplicateIndices = new List<int>();

            for (int s = 0; s < route.Stops.Count; s++)
            {
                if (seenStops.Contains(route.Stops[s]))
                {
                    duplicateIndices.Add(s);
                }
                else
                {
                    seenStops.Add(route.Stops[s]);
                }
            }

            for (int s = 0; s < route.Stops.Count; s++)
            {
                if (duplicateIndices.Contains(s))
                {
                    route.StopTimes.RemoveAt(s);
                    route.Stops.RemoveAt(s);
                    route.Addresses?.RemoveAt(s);
                    route.Prices?.RemoveAt(s);
                }
            }

            return route;
        }

        private async Task<Tuple<bool, DateTime>> TryEnforceStopTimeOptionsAsync(
            Place from,
            Place to,
            Mode mode,
            DateTime utcDeparture,
            DateTime utcArrival,
            StopTimeOptions options)
        {
            if (options == StopTimeOptions.None)
            {
                return new Tuple<bool, DateTime>(true, utcArrival);
            }

            var invalidOptions = 
                options & 
                StopTimeOptions.EstimateDuplicates & 
                StopTimeOptions.RemoveDuplicates;

            if (invalidOptions > 0)
            {
                throw new InvalidOperationException("Invalid options.");
            }

            if (options.Matches(StopTimeOptions.RemoveDuplicates) && 
                utcDeparture.Equals(utcArrival))
            {
                return new Tuple<bool, DateTime>(false, utcArrival);
            }

            if (options.Matches(StopTimeOptions.EstimateDuplicates) && 
                utcDeparture.Equals(utcArrival))
            {
                var arrival = await this.estimator.EstimateArrivalTimeAsync(
                    from, to, utcDeparture, mode);

                return new Tuple<bool, DateTime>(true, arrival);
            }

            if (options.Matches(StopTimeOptions.AdjustInvalidArrivals) && 
                utcDeparture > utcArrival)
            {
                return new Tuple<bool, DateTime>(true, utcDeparture.AddMinutes(1));
            }

            return new Tuple<bool, DateTime>(true, utcArrival);
        }
    }
}
