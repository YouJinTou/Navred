using System;
using System.Collections.Generic;

namespace Navred.Core.Search
{
    public class Vertex : IEquatable<Vertex>
    {
        public string Name { get; set; }

        public IEnumerable<Edge> Edges { get; set; }

        public bool Equals(Vertex other) => this.Name.Equals(other?.Name);

        public override string ToString() => this.Name;
    }
}
