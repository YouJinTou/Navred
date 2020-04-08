using Navred.Core.Tools;
using System;
using System.Linq;

namespace Navred.Core.Search.Algorithms
{
    public class KShortestPaths
    {
        public GraphSearchResult FindKShortestPaths(Graph g, int k)
        {
            Validator.ThrowIfNull(g, "Graph is empty.");

            var dijkstra = new Dijkstra();

            var result = new GraphSearchResult();

            for (int i = 0; i < k; i++)
            {
                var path = dijkstra.FindShortestPath(g);

                result.Add(path);

                var modifiable = g.Edges.First(e => e.Equals(path.Tail));
                modifiable.Weight += new Weight { Duration = TimeSpan.FromMinutes(5) };
            }

            return result;
        }
    }
}
