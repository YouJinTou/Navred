using Navred.Core.Extensions;
using Navred.Core.Search;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Tests.Search
{
    public class GraphBuilder
    {
        private readonly Random rand;
        private Vertex source;
        private Vertex destination;
        private IEnumerable<Vertex> vertices;
        private IEnumerable<Edge> edges;

        public GraphBuilder()
        {
            this.rand = new Random();
        }

        public GraphBuilder BuildGraph()
        {
            return this;
        }

        public GraphBuilder WithVertices(int n)
        {
            if (n <= 1)
            {
                throw new ArgumentException("Number of vertices must be greater than two.");
            }

            var vertices = new List<Vertex>();

            for (int i = 0; i < n; i++)
            {
                vertices.Add(new Vertex
                {
                    Name = this.rand.NextDouble().ToString()
                });
            }

            this.source = vertices.First();
            this.destination = vertices.Last();
            this.vertices = vertices;

            return this;
        }

        public GraphBuilder WithEdges(int n, int paths)
        {
            if (this.vertices.IsNullOrEmpty())
            {
                throw new InvalidOperationException("Cannot add edges before vertices.");
            }

            if (paths >= n)
            {
                throw new InvalidOperationException("Paths more than edges.");
            }

            var edges = new List<Edge>();

            for (int i = 0; i < n; i++)
            {
                edges.Add(new Edge
                {
                    //Source = this.source,
                    //Destination = this.destination
                });
            }

            this.edges = edges;

            return this;
        }

        public Graph Build()
        {
            var graph = new Graph(this.source, this.destination, this.vertices, this.edges);

            return graph;
        }
    }
}
