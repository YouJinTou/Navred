using System.Collections.Generic;

namespace Navred.Core.Search
{
    internal class Graph
    {
        public IEnumerable<Vertex> Vertices { get; set; }

        public IEnumerable<Edge> Edges { get; set; }
    }
}
