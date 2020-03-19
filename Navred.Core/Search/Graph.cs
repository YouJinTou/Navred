using System.Collections.Generic;

namespace Navred.Core.Search
{
    public class Graph
    {
        public Graph(Vertex source, IEnumerable<Vertex> vertices, IEnumerable<Edge> edges)
        {
            this.Source = source;
            this.Vertices = vertices;
            this.Edges = edges;
        }

        public Vertex Source { get; }

        public IEnumerable<Vertex> Vertices { get; }

        public IEnumerable<Edge> Edges { get; }
    }
}
