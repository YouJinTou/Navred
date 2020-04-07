using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    internal class GraphSearchPathEqualityComparer : IEqualityComparer<GraphSearchPath>
    {
        public bool Equals(GraphSearchPath x, GraphSearchPath y)
        {
            if (!x.Path.Count.Equals(y.Path.Count))
            {
                return false;
            }

            var xHead = x.Path.First();
            var yHead = y.Path.First();
            var xTail = x.Path.Last();
            var yTail = y.Path.Last();

            if (!xHead.Source.Equals(yHead.Source)) return false;
            if (!xHead.Destination.Equals(yHead.Destination)) return false;
            if (!xHead.Leg.UtcDeparture.Equals(yHead.Leg.UtcDeparture)) return false;
            if (!xHead.Leg.UtcArrival.Equals(yHead.Leg.UtcArrival)) return false;
            if (!xTail.Leg.UtcDeparture.Equals(yTail.Leg.UtcDeparture)) return false;
            if (!xTail.Leg.UtcArrival.Equals(yTail.Leg.UtcArrival)) return false;
            if (!xHead.Leg.Carrier.Equals(yHead.Leg.Carrier)) return false;
            if (!xTail.Leg.Carrier.Equals(yTail.Leg.Carrier)) return false;

            return true;
        }

        public int GetHashCode(GraphSearchPath p)
        {
            int prime = 83;
            int result = 1;
            var sourceDestination = $"{p.Source} - {p.Destination}";

            unchecked
            {
                result *= prime + p.Weight.Duration.ToString().GetHashCode();
                result *= prime + sourceDestination.GetHashCode();
            }

            return result;
        }
    }
}
