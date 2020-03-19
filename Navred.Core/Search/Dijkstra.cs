using Navred.Core.Itineraries;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    internal class Dijkstra : IPathFinder
    {
        public IEnumerable<Itinerary> FindItineraries(Graph graph)
        {
            var weights = graph.Vertices.ToDictionary(kvp => kvp.Name, kvp => Weight.CreateMax());
            weights[graph.Source.Name] = new Weight();

            foreach (var edge in graph.Source.Edges)
            {
                weights[edge.Destination.Name] = edge.Weight;
            }

            this.UpdateWeightsRecursive(graph.Source, weights);

            return null;
        }

        private void UpdateWeightsRecursive(Vertex vertex, IDictionary<string, Weight> weights)
        {
            var cheapestDestinationVertex = default(Vertex);
            var q = default(Weight);

            foreach (var e in vertex.Edges)
            {
                e.Weight
            }
        }
    }
}
