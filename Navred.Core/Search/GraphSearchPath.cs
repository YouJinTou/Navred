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

            this.Weight = this.Weight == null ? new Weight() : this.Weight;
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

        public bool Contains(Edge edge)
        {
            return this.Path.Any(e => e.Source == edge.Destination);
        }

        public override string ToString()
        {
            var destination = this.Path.Last().Destination.ToString();
            var stops = string.Join(" - ", this.Path.Select(p => p.Source));
            var result = $"{stops} - {destination} | {this.Weight}";

            return result;
        }
    }
}
