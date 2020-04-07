using Navred.Core.Itineraries;
using System;

namespace Navred.Core.Search
{
    public class Edge : IEquatable<Edge>
    {
        public Vertex Source { get; set; }

        public Vertex Destination { get; set; }

        public Weight Weight { get; set; }

        public Leg Leg { get; set; }

        public bool ArrivesAfterHasDeparted(Edge other)
        {
            if (!this.Leg.ToId.Equals(other.Leg.FromId))
            {
                throw new InvalidOperationException("Destination and source must match.");
            }

            return this.Leg.UtcArrival >= other.Leg.UtcDeparture;
        }

        public bool TryMergeWith(Edge other, out Edge merged)
        {
            merged = null;

            if (!this.Leg.TryMergeWith(other?.Leg, out Leg m))
            {
                return false;
            }

            merged = new Edge
            {
                Source = this.Source,
                Destination = other.Destination,
                Leg = m,
                Weight = new Weight
                {
                    Duration = m.Duration,
                    Price = m.Price
                }
            };

            return true;
        }

        public override string ToString() => this.Leg?.ToString() ?? 
            $"{this.Source} - {this.Destination}";

        public bool Equals(Edge other) => 
            this.Source.Equals(other.Source) && 
            this.Destination.Equals(other.Destination) && 
            this.Weight.Equals(other.Weight);
    }
}
