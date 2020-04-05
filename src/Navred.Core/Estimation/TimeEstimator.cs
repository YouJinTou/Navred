using Navred.Core.Itineraries;
using Navred.Core.Places;
using Navred.Core.Tools;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Navred.Core.Estimation
{
    public class TimeEstimator : ITimeEstimator
    {
        private const double RoadCurvatureRate = 1.2d;
        private const double BusAverageKmPerHour = 80d;
        private const double SlackRatePerKilometer = 1.1d;

        private readonly IHttpClientFactory httpClientFactory;
        private readonly IDictionary<string, double> distancesCache;

        public TimeEstimator(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            this.distancesCache = new Dictionary<string, double>();
        }

        public async Task<DateTime> EstimateArrivalTimeAsync(
            Place from, Place to, DateTime departure, Mode mode)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to);

            var arrival = await Task.FromResult(this.EstimateManually(from, to, departure, mode));

            return arrival;
        }

        private DateTime EstimateManually(Place from, Place to, DateTime departure, Mode mode)
        {
            var distance = from.DistanceToInKm(to);

            if (distance == 0d)
            {
                return departure;
            }

            double hours;

            switch (mode)
            {
                case Mode.Bus:
                    hours = distance / BusAverageKmPerHour;

                    break;
                default:
                    throw new NotImplementedException();
            }

            var variableDuration =
                (int)((((int)Math.Ceiling(hours * 60)) * RoadCurvatureRate * SlackRatePerKilometer));
            var duration = new TimeSpan(0, variableDuration, 0);
            var arrival = departure + duration;

            return arrival;
        }
    }
}
