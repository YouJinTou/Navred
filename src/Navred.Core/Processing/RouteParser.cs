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
            Route route, StopTimeOptions stopTimeOptions = StopTimeOptions.None)
        {
            var preprocessedRoute = this.PreprocessRoute(route);
            var stops = this.placesManager.DeducePlacesFromStops(
                this.cultureProvider.Name, route.Stops, false);
            var schedule = new Schedule();

            foreach (var (current, next) in stops.AsPairs())
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
                            current.Place, 
                            next.Place, 
                            route.Mode, 
                            departureTimes[t], 
                            arrivalTimes[t], 
                            stopTimeOptions);

                        if (enforceResult.Item1)
                        {
                            var leg = new Leg(
                                from: current.Place,
                                to: next.Place,
                                utcDeparture: departureTimes[t],
                                utcArrival: enforceResult.Item2,
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
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, current.ToString());
                }
            }

            var result = schedule.Permute();

            return result;
        }

        private Route PreprocessRoute(Route route)
        {
            Validator.ThrowIfNull(route, "Empty route.");

            var copy = route.Copy();

            return this.RemoveDuplicateStops(copy);
        }

        private Route RemoveDuplicateStops(Route route)
        {
            var uniqueStops = new HashSet<Stop>(route.Stops);
            var copy = route.Copy(uniqueStops);

            return copy;
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
