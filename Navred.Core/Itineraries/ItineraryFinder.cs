using Navred.Core.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries
{
    public class ItineraryFinder
    {
        public async Task<IEnumerable<Itinerary>> FindItinerariesAsync(string from, string to, IEnumerable<Itinerary> temp)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to);

            return null;
        }
    }
}
