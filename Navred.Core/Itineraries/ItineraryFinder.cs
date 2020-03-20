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
        private readonly IPathFinder pathFinder;

        public ItineraryFinder(IItineraryRepository repo, IPathFinder pathFinder)
        {
            this.repo = repo;
            this.pathFinder = pathFinder;
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
                    Carrier = i.Carrier,
                    Duration = i.Duration,
                    Price = i.Price,
                    UtcArrival = i.UtcArrival
                }
            }).ToList();

            foreach (var vertex in vertices)
            {
                vertex.Edges = edges.Where(e => e.Source.Name == vertex.Name).ToList();
            }

            var graph = new Graph(
                vertices.Single(v => v.Name == from), 
                vertices.Single(v => v.Name == to),
                vertices, 
                edges);
            var result = this.pathFinder.FindItineraries(graph);

            return result;
        }
    }
}
