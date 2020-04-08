using Navred.Core.Extensions;
using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search.Algorithms
{
    public class Dijkstra
    {
        public GraphSearchPath FindShortestPath(Graph g)
        {
            Validator.ThrowIfNull(g, "Graph is empty.");

            var unvisited = new HashSet<Vertex>(g.Vertices);
            var distances = g.Vertices.ToDictionary(kvp => kvp, kvp => Weight.Max());
            var previous = g.Vertices.ToDictionary(kvp => kvp, kvp => default(Vertex));
            var paths = g.Vertices.ToDictionary(kvp => kvp, kvp => new List<Edge>());
            distances[g.Source] = Weight.Zero();

            while (!unvisited.IsEmpty())
            {
                var current = distances.Where(
                    d => unvisited.Contains(d.Key)).GetMin(d => d.Value).Key;

                foreach (var e in current.Edges)
                {
                    var currentDistance = distances[current] + e.Weight;

                    if (currentDistance < distances[e.Destination])
                    {
                        distances[e.Destination] = currentDistance;
                        previous[e.Destination] = current;
                        paths[e.Destination] = new List<Edge>(paths[current]) { e };
                    }
                }

                unvisited.Remove(current);
            }

            var path = new GraphSearchPath(paths[g.Destination]);

            return path;
        }
    }
}
