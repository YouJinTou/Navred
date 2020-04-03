using System.Collections.Generic;

namespace Navred.Core.Search
{
    internal class WeightEqualityComparer : IEqualityComparer<Weight>
    {
        public bool Equals(Weight x, Weight y)
        {
            return x.Duration.Equals(y.Duration) && x.Price.Equals(y.Price);
        }

        public int GetHashCode(Weight w)
        {
            int prime = 83;
            int result = 1;

            unchecked
            {
                result *= prime + w.Duration.GetHashCode();
                result *= prime + w.Price?.GetHashCode() ?? prime;
            }

            return result;
        }
    }
}
