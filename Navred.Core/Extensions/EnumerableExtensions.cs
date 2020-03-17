using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null || enumerable.Count() == 0)
            {
                return true;
            }

            return false;
        }
    }
}
