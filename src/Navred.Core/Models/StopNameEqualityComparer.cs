using System.Collections.Generic;

namespace Navred.Core.Models
{
    public class StopNameEqualityComparer : IEqualityComparer<Stop>
    {
        public bool Equals(Stop x, Stop y)
        {
            return x.CompositeName.ToLower().Equals(y.CompositeName.ToLower());
        }

        public int GetHashCode(Stop obj)
        {
            int prime = 83;
            int result = 1;

            unchecked
            {
                result *= prime + obj.Name.ToLower().GetHashCode();
                result *= prime + obj.Region?.ToLower()?.GetHashCode() ?? prime;
                result *= prime + obj.Municipality?.ToLower()?.GetHashCode() ?? prime;
            }

            return result;
        }
    }
}
