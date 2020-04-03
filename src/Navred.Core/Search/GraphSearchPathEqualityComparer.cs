using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Search
{
    internal class GraphSearchPathEqualityComparer : IEqualityComparer<GraphSearchPath>
    {
        public bool Equals(GraphSearchPath x, GraphSearchPath y)
        {
            return
                x.Weight.Equals(y.Weight) &&
                $"{x.Source} - {x.Destination} | {x.Path.Last().Leg.Carrier}"
                .Equals($"{y.Source} - {y.Destination} | {y.Path.Last().Leg.Carrier}");
        }

        public int GetHashCode(GraphSearchPath p)
        {
            int prime = 83;
            int result = 1;
            var sourceDestination = $"{p.Source} - {p.Destination}";

            unchecked
            {
                result *= prime + p.Weight.ToString().GetHashCode();
                result *= prime + sourceDestination.GetHashCode();
            }

            return result;
        }
    }
}
