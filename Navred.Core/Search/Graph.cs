using Navred.Core.Tools;
using System.Collections.Generic;

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
    }
}
