using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Count() == 0;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null || IsEmpty(enumerable))
            {
                return true;
            }

            return false;
        }
    }
}
