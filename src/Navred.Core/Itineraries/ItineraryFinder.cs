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
        private readonly ILegRepository repo;

        public ItineraryFinder(ILegRepository repo)
        {
            this.repo = repo;
        }

        public async Task<IEnumerable<Leg>> FindItinerariesAsync(
            string from, string to, TimeWindow window)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to, window);

            var legs = await this.repo.GetLegsAsync(from, to, window);
            var vertices = legs
                .Select(i => new List<string> { i.From, i.To })
                .SelectMany(s => s)
                .Distinct()
                .Select(s => new Vertex { Name = s })
                .ToList();
            var edges = legs.Select(l => new Edge
            {
                Carrier = l.Carrier,
                Source = vertices.Single(v => v.Name == l.From),
                Destination = vertices.Single(v => v.Name == l.To),
                Weight = new Weight
                {
                    Duration = l.Duration,
                    Price = l.Price,
                },
                UtcArrival = l.UtcArrival,
                UtcDeparture = l.UtcDeparture
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
            var result = graph.FindAllPaths(graph.Source, graph.Destination);
            var resultLegs = result.Paths
                .Select(p => p.Path)
                .SelectMany(e => e)
                .Select(e => new Leg(
                    e.Source.Name,
                    e.Destination.Name,
                    e.UtcDeparture,
                    e.UtcArrival,
                    e.Carrier,
                    e.Mode,
                    e.Weight.Price,
                    e.FromSpecific,
                    e.ToSpecific))
                .ToList();

            return resultLegs;
        }
    }
}
