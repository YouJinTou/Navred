using Navred.Core.Itineraries.DB;
using Navred.Core.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries
{
    public class ItineraryFinder : IItineraryFinder
    {
        private readonly IItineraryRepository repo;

        public ItineraryFinder(IItineraryRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IEnumerable<Itinerary>> FindItinerariesAsync(
            string from, string to, TimeWindow window)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to, window);

            return null;
        }
    }
}
