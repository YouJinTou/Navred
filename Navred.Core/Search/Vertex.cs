using System;
using System.Collections.Generic;

namespace Navred.Core.Search
{
    public class Vertex : IEquatable<Vertex>
    {
        public string Name { get; set; }

        public IEnumerable<Edge> Edges { get; set; }

        public bool Equals(Vertex other)
        {
            return this.Name.Equals(other?.Name);
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
