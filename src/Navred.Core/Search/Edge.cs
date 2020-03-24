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

        public override string ToString()
        {
            return this.Leg.ToString();
        }
    }
}
