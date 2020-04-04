using Navred.Core.Itineraries;
using Navred.Core.Places;
using System;
using System.Threading.Tasks;

namespace Navred.Core.Piecing
{
    public interface ITimeEstimator
    {
        Task<DateTime> EstimateArrivalTimeAsync(
            Place from, Place to, DateTime departure, Mode mode);
    }
}