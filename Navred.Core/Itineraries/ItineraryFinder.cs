using Navred.Core.Itineraries;
using Navred.Core.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Search
{
    public class ItineraryFinder
    {
        public async Task<IEnumerable<Itinerary>> FindItineraryAsync(string from, string to)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to);

            return null;
        }
    }
}
