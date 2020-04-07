using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Core.Search
{
    public class Graph
    {
        public Graph(
            Vertex source, 
            Vertex destination, 
            IEnumerable<Vertex> vertices, 
            IEnumerable<Edge> edges)
        {
            this.Source = Validator.ReturnOrThrowIfNull(source);
            this.Destination = Validator.ReturnOrThrowIfNull(destination);
            this.Vertices = Validator.ReturnOrThrowIfNull(vertices);
            this.Edges = Validator.ReturnOrThrowIfNull(edges);
        }

        public Vertex Source { get; }

        public Vertex Destination { get; }

        public IEnumerable<Vertex> Vertices { get; }

        public IEnumerable<Edge> Edges { get; }

        public GraphSearchResult FindAllPaths(Vertex source, Vertex destination)
        {
            var result = new GraphSearchResult();

            Parallel.ForEach(source.Edges, e =>
            {
                this.FindAllPathsRecursive(e, destination, new GraphSearchPath(), result);
            });

            var finalResult = result.Merge().Filter().Sort();

            return finalResult;
        }

        private void FindAllPathsRecursive(
            Edge edge,
            Vertex destination, 
            GraphSearchPath currentPath, 
            GraphSearchResult result)
        {
            currentPath.Add(edge);

            if (edge.Destination.Equals(destination))
            {
                result.Add(currentPath);

                return;
            }

            foreach (var e in edge.Destination.Edges)
            {
                if (currentPath.Contains(e) || currentPath.Tail.ArrivesAfterHasDeparted(e))
                {
                    continue;
                }

                this.FindAllPathsRecursive(e, destination, currentPath, result);

                currentPath.Remove(e);
            }
        }

        public override string ToString()
        {
            return $"Vertices: {this.Vertices.Count()} Edges: {this.Edges.Count()}";
        }
    }
}
