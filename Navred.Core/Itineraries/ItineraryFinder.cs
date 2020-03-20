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
                    Price = i.Price,
                },
                UtcArrival = i.UtcArrival,
                UtcDeparture = i.UtcDeparture
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
            var resultItineraries = new List<Itinerary>();

            foreach (var path in result.Paths)
            {
                var itinerary = new Itinerary();

                foreach (var edge in path.Path)
                {
                    itinerary.AddStop(new Stop(edge.Source.Name, edge.UtcArrival, edge.Carrier, edge.Weight.Price));
                }

                resultItineraries.Add(itinerary);
            }

            return resultItineraries;
        }
    }
}
