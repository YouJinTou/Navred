using Navred.Core.Places;
using Navred.Core.Search;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries
{
    public interface IItineraryFinder
    {
        Task<IEnumerable<GraphSearchPath>> FindItinerariesAsync(
            Place from, Place to, TimeWindow window);
    }
}