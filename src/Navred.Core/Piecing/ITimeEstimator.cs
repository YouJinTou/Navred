using Navred.Core.Itineraries;
using Navred.Core.Places;
using System;

namespace Navred.Core.Piecing
{
    public interface ITimeEstimator
    {
        DateTime EstimateArrivalTime(IPlace from, IPlace to, DateTime departure, Mode mode);
    }
}