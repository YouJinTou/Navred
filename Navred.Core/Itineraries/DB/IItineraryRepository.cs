using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries.DB
{
    public interface IItineraryRepository
    {
        Task<IEnumerable<Itinerary>> GetItinerariesAsync(
            string from, string to, TimeWindow window);

        Task UpdateItinerariesAsync(IEnumerable<Itinerary> itineraries);
    }
}