using Navred.Core.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    public class GraphSearchPath
    {
        public ICollection<Edge> Path { get; private set; }

        public Weight Weight { get; private set; }

        public void Add(Edge edge)
        {
            if (this.Path.IsNullOrEmpty())
            {
                this.Path = new List<Edge>();
            }

            this.Path.Add(edge);

            this.Weight = this.Weight ?? new Weight();
            this.Weight += edge.Weight;
        }

        public void Remove(Edge edge)
        {
            this.Path.Remove(edge);
        }

        public GraphSearchPath Copy()
        {
            var cost = this.Weight == null ? null : new Weight
            {
                Duration = this.Weight.Duration,
                Price = this.Weight.Price
            };

            return new GraphSearchPath
            {
                Weight = cost,
                Path = new List<Edge>(this.Path)
            };
        }

        public Vertex GetSource()
            => this.Path.First().Source;

        public Vertex GetDestination()
            => this.Path.Last().Destination;

        public bool Contains(Edge edge)
        {
            return this.Path.Any(e => e.Source == edge.Destination);
        }

        public override string ToString()
        {
            var destination = this.GetDestination().ToString();
            var legs = string.Join(" - ", this.Path.Select(p => p.Source));
            var result = $"{legs} - {destination} | {this.Weight}";

            return result;
        }
    }
}
