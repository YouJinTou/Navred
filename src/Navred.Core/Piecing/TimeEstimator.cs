using Navred.Core.Itineraries;
using Navred.Core.Places;
using Navred.Core.Tools;
using System;

namespace Navred.Core.Piecing
{
    public class TimeEstimator : ITimeEstimator
    {
        private const double RoadCurvatureRate = 1.2;
        private const double BusAverageKmPerHour = 80d;
        private const int BusSlackInMinutes = 30;

        public DateTime EstimateArrivalTime(IPlace from, IPlace to, DateTime departure, Mode mode)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to);

            var distance = from.DistanceToInKm(to);
            var hours = 0d;

            switch (mode)
            {
                case Mode.Bus:
                    hours = distance / BusAverageKmPerHour;

                    break;
                default:
                    throw new NotImplementedException();
            }

            var variableDuration =
                (int)(((int)Math.Ceiling(hours * 60)) * RoadCurvatureRate) + BusSlackInMinutes;
            var duration = new TimeSpan(0, variableDuration, 0);
            var arrival = departure + duration;

            return arrival;
        }
    }
}
