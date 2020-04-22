using System.Collections.Generic;

namespace Navred.Core.Models
{
    public class StopTimeEqualityComparer : IEqualityComparer<Stop>
    {
        public bool Equals(Stop x, Stop y)
        {
            return
                x.CompositeName.ToLower().Equals(y.CompositeName.ToLower()) &&
                x.Time.Equals(y.Time);
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
                result *= prime + obj.Time?.GetHashCode() ?? prime;
            }

            return result;
        }
    }
}
