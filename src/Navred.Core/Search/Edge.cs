using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    public class Edge : IEquatable<Edge>, ICopyable<Edge>
    {
        public Vertex Source { get; set; }

        public Vertex Destination { get; set; }

        public Weight Weight { get; set; }

        public Leg Leg { get; set; }

        public Edge Reverse()
        {
            return new Edge
            {
                Destination = this.Source.Copy(),
                Weight = this.Weight.Copy(),
                Source = this.Destination.Copy(),
                Leg = this.Leg.CopyReverse()
            };
        }

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
            $"{this.Source} - {this.Destination} | {this.Weight}";

        public bool Equals(Edge other) => 
            this.Source.Equals(other.Source) && 
            this.Destination.Equals(other.Destination) && 
            this.Leg.Carrier.Equals(other.Leg.Carrier) &&
            this.Leg.UtcDeparture.Equals(other.Leg.UtcDeparture) &&
            this.Leg.UtcArrival.Equals(other.Leg.UtcArrival) &&
            this.Weight.Equals(other.Weight);

        public override int GetHashCode()
        {
            int prime = 83;
            int result = 1;

            unchecked
            {
                result *= prime + this.Source.GetHashCode();
                result *= prime + this.Destination.GetHashCode();
                result *= prime + this.Leg.Carrier.GetHashCode();
                result *= prime + this.Leg.UtcDeparture.GetHashCode();
                result *= prime + this.Leg.UtcArrival.GetHashCode();
                result *= prime + this.Weight.GetHashCode();
            }

            return result;
        }

        public Edge Copy()
        {
            return new Edge
            {
                Source = this.Source.Copy(),
                Destination = this.Destination.Copy(),
                Weight = this.Weight.Copy(),
                Leg = this.Leg.Copy()
            };
        }

        public Edge FindClosestInTime(IEnumerable<Edge> edges)
        {
            var diffs = new Dictionary<Edge, TimeSpan>();

            foreach (var e in edges)
            {
                var arrivalDiff = (this.Leg.UtcArrival - e.Leg.UtcArrival).Duration();
                var departureDiff = (this.Leg.UtcDeparture - e.Leg.UtcDeparture).Duration();
                var diff = arrivalDiff + departureDiff;

                diffs.Add(e, diff);
            }

            var closest = diffs.GetMin(d => d.Value);

            return closest.Key;
        }
    }
}
