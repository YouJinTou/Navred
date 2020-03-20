using System.Collections.Generic;
using System.Threading.Tasks;
using Navred.Core.Itineraries.DB;

namespace Navred.Core.Itineraries
{
    public interface IItineraryFinder
    {
        Task<IEnumerable<Leg>> FindItinerariesAsync(string from, string to, TimeWindow window);
    }
}