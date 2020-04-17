using Navred.Core.Extensions;
using System.Threading.Tasks;

namespace Navred.Core.Search.Algorithms
{
    public class Dfs
    {
        public GraphSearchResult FindAllPaths(Graph g)
        {
            g.ThrowIfNull("Graph is empty.");

            var result = new GraphSearchResult();

            Parallel.ForEach(g.Source.Edges, e =>
            {
                this.FindAllPathsRecursive(e, g.Destination, new GraphSearchPath(), result);
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
                if (currentPath.Touches(e) || currentPath.Tail.ArrivesAfterHasDeparted(e))
                {
                    continue;
                }

                this.FindAllPathsRecursive(e, destination, currentPath, result);

                currentPath.Remove(e);
            }
        }
    }
}
