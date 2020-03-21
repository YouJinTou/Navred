using Navred.Core.Itineraries;
using System;

namespace Navred.Core.Search
{
    public class Edge
    {
        public Vertex Source { get; set; }

        public Vertex Destination { get; set; }

        public Weight Weight { get; set; }

        public DateTime UtcArrival { get; set; }

        public DateTime UtcDeparture { get; set; }

        public string Carrier { get; set; }

        public Mode Mode { get; set; }

        public string FromSpecific { get; set; }

        public string ToSpecific { get; set; }

        public override string ToString()
        {
            return 
                $"{this.Source} {this.UtcDeparture} - " +
                $"{this.Destination} {this.UtcArrival} | {this.Weight}";
        }
    }
}
