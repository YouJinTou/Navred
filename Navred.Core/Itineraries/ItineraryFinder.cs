using Navred.Core.Itineraries.DB;
using Navred.Core.Search;
using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;
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

            var itineraries = await this.repo.GetItinerariesAsync(from, to, window);
            var vertices = itineraries
                .Select(i => new List<string> { i.From, i.To })
                .SelectMany(s => s)
                .Distinct()
                .Select(s => new Vertex { Name = s })
                .ToList();
            var edges = itineraries.Select(i => new Edge
            {
                Source = vertices.First(v => v.Name == i.From),
                Destination = vertices.First(v => v.Name == i.To),
                Weight = new Weight
                {
                    Duration = i.Duration,
                    Price = i.Price
                }
            }).ToList();
            var graph = new Graph
            {
                Edges = edges,
                Vertices = vertices
            };

            return itineraries;
        }
    }
}
