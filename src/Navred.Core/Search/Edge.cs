using Navred.Core.Itineraries;
using System;

namespace Navred.Core.Search
{
    public class Edge
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

        public override string ToString() => this.Leg.ToString();
    }
}
