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

            this.Weight = this.Weight ?? new Weight();
            this.Weight += edge.Weight;

            this.AddWaitTime(edge);

            this.Path.Add(edge);
        }

        public void Remove(Edge edge)
        {
            this.Weight -= edge.Weight;

            this.Path.Remove(edge);

            this.RemoveWeightTime(edge);
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

        public Vertex Source => this.Path.First().Source;

        public Vertex Destination => this.Path.Last().Destination;

        public Edge Tail => this.Path.Last();

        public bool Contains(Edge edge)
            => this.Path.Any(e => e.Source == edge.Destination);

        private void AddWaitTime(Edge edge)
        {
            if (this.Path.IsEmpty())
            {
                return;
            }

            this.Weight += new Weight
            {
                Duration = edge.Leg.UtcDeparture - this.Path.Last().Leg.UtcArrival,
                Price = null
            };
        }

        private void RemoveWeightTime(Edge edge)
        {
            if (this.Path.IsEmpty())
            {
                return;
            }

            this.Weight -= new Weight
            {
                Duration = edge.Leg.UtcDeparture - this.Path.Last().Leg.UtcArrival,
                Price = null
            };
        }

        public override string ToString()
        {
            var destination = this.Destination.ToString();
            var legs = string.Join(" - ", this.Path.Select(p => p.Source));
            var result = $"{legs} - {destination} | {this.Weight}";

            return result;
        }
    }
}
