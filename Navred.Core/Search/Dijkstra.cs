using Navred.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    internal class Dijkstra : IPathFinder
    {
        public GraphSearchResult FindPaths(Graph graph)
        {
            return null;
            //var distances = graph.Vertices.ToDictionary(
            //    kvp => kvp.Name, kvp => Weight.CreateMax());
            //distances[graph.Source.Name] = new Weight();
            //var unvisited = new HashSet<Vertex>(graph.Vertices);
            //var previous = graph.Vertices.ToDictionary(
            //    kvp => kvp.Name, kvp => kvp.Name);

            //this.UpdateDistancesRecursive(graph.Source, unvisited, distances, previous);

            //var result = new GraphSearchResult
            //{
            //    Paths = this.GetPaths(graph, previous, distances)
            //};

            //return result;
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

        private IEnumerable<GraphSearchPath> GetPaths(
            Graph graph,
            IDictionary<string, string> previous,
            IDictionary<string, Weight> distances)
        {
            return null;
            //var last = previous[graph.Destination.Name];
            //var path = new GraphSearchPath
            //{
            //    Path = new List<Vertex>()
            //};

            //path.Path.Add(graph.Vertices.First(v => v.Name == graph.Destination.Name));

            //while (last != graph.Source.Name)
            //{
            //    var distance = distances[last];
            //    last = previous[last];

            //    path.Path.Add(graph.Vertices.First(v => v.Name == last));
            //}

            //path.Path.Add(graph.Vertices.First(v => v.Name == graph.Source.Name));

            //path.Path.Reverse();

            //return new List<GraphSearchPath> { path };
        }
    }
}
