using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    internal class Dijkstra : IPathFinder
    {
        public IEnumerable<Itinerary> FindItineraries(Graph graph)
        {
            var distances = graph.Vertices.ToDictionary(
                kvp => kvp.Name, kvp => Weight.CreateMax());
            distances[graph.Source.Name] = new Weight
            {
                UtcArrival = graph.Source.Edges.First().Weight.UtcArrival
            };
            var unvisited = new HashSet<Vertex>(graph.Vertices);
            var previous = graph.Vertices.ToDictionary(
                kvp => kvp.Name, kvp => kvp.Name);

            this.UpdateDistancesRecursive(graph.Source, unvisited, distances, previous);

            var itineraries = this.GetItineraries(graph, previous, distances);

            return itineraries;
        }

        private void UpdateDistancesRecursive(
            Vertex vertex, 
            ICollection<Vertex> unvisited, 
            IDictionary<string, Weight> distances, 
            IDictionary<string, string> previous)
        {
            if (unvisited.IsEmpty())
            {
                return;
            }

            foreach (var edge in vertex.Edges)
            {
                var currentDistance = distances[vertex.Name] + edge.Weight;

                if (currentDistance < distances[edge.Destination.Name])
                {
                    distances[edge.Destination.Name] = currentDistance;
                    previous[edge.Destination.Name] = edge.Source.Name;
                }
            }

            unvisited.Remove(vertex);

            var minWeight = Weight.CreateMax();
            var minVertex = default(Vertex);

            foreach (var u in unvisited)
            {
                var distance = distances[u.Name];

                if (distance < minWeight)
                {
                    minWeight = distance;
                    minVertex = u;
                }
            }

            this.UpdateDistancesRecursive(minVertex, unvisited, distances, previous);
        }

        private IEnumerable<Itinerary> GetItineraries(
            Graph graph,
            IDictionary<string, string> previous, 
            IDictionary<string, Weight> distances)
        {
            var last = previous[graph.Destination.Name];
            var stops = new List<Stop>
            {
                new Stop(graph.Destination.Name, distances[graph.Destination.Name].UtcArrival)
            };

            while (last != graph.Source.Name)
            {
                var distance = distances[last];

                stops.Add(new Stop(last, distance.UtcArrival));

                last = previous[last];
            }

            stops.Add(new Stop(graph.Source.Name, distances[graph.Source.Name].UtcArrival));

            stops.Reverse();

            var bestItinerary = new Itinerary("AN ITINERARY CAN CONTAIN MORE THAN ONE CARRIER...");

            bestItinerary.AddStops(stops);

            return new List<Itinerary> { bestItinerary };
        }
    }
}
