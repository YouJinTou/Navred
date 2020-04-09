using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using System;
using System.Collections.Generic;

namespace Navred.Core.Search
{
    public class Vertex : IEquatable<Vertex>, ICopyable<Vertex>
    {
        public Vertex()
        {
            this.Edges = new List<Edge>();
        }

        public string Name { get; set; }

        public ICollection<Edge> Edges { get; }

        public void AddEdge(Edge edge)
        {
            this.Edges.Add(edge);
        }

        public void AddEdges(IEnumerable<Edge> edges)
        {
            foreach (var e in edges)
            {
                this.AddEdge(e);
            }
        }

        public bool Equals(Vertex other) => this.Name.Equals(other?.Name);

        public override string ToString() => this.Name.FormatId();

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var vertex = obj as Vertex;

            if (vertex == null)
            {
                return false;
            }

            return this.Name.Equals(vertex.Name);
        }

        public Vertex Copy()
        {
            var v = new Vertex { Name = this.Name };

            foreach (var e in this.Edges)
            {
                var destination = new Vertex { Name = e.Destination.Name };

                v.Edges.Add(new Edge
                {
                    Source = v,
                    Destination = destination,
                    Weight = e.Weight.Copy(),
                    Leg = e.Leg.Copy()
                });
            }

            return v;
        }
    }
}
