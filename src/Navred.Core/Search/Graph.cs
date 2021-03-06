﻿using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    public class Graph : ICopyable<Graph>
    {
        public Graph(
            Vertex source,
            Vertex destination,
            IEnumerable<Vertex> vertices,
            IEnumerable<Edge> edges)
        {
            this.Source = source.ReturnOrThrowIfNull();
            this.Destination = destination.ReturnOrThrowIfNull();
            this.Vertices = vertices ?? new List<Vertex>();
            this.Edges = new HashSet<Edge>(edges ?? new List<Edge>());

            if (this.Source.Edges.IsEmpty())
            {
                this.Source.AddEdges(this.Edges.Where(e => e.Source.Equals(this.Source)));
            }

            if (this.Destination.Edges.IsEmpty())
            {
                this.Destination.AddEdges(
                    this.Edges.Where(e => e.Source.Equals(this.Destination)));
            }

            foreach (var v in this.Vertices)
            {
                if (v.Edges.IsEmpty())
                {
                    v.AddEdges(this.Edges.Where(e => e.Source.Equals(v)));
                }
            }
        }

        public Vertex Source { get; private set; }

        public Vertex Destination { get; private set; }

        public IEnumerable<Vertex> Vertices { get; }

        public IEnumerable<Edge> Edges { get; private set; }

        public Graph Copy()
        {
            var graph = new Graph(
               this.Source.Copy(),
               this.Destination.Copy(),
               this.Vertices.Select(v => v.Copy()),
               this.Edges.Select(e => e.Copy()));

            return graph;
        }

        public Graph Reverse()
        {
            var source = new Vertex { Name = this.Destination.Name };
            var destination = new Vertex { Name = this.Source.Name };
            var vertices = this.Vertices.Select(v => new Vertex { Name = v.Name }).ToList();
            var edges = this.Edges.Select(e => e.Reverse()).ToList();
            var graph = new Graph(source, destination, vertices, edges);

            return graph;
        }

        public override string ToString()
        {
            return $"Vertices: {this.Vertices.Count()} Edges: {this.Edges.Count()}";
        }
    }
}
