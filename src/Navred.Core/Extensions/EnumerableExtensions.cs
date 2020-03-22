using System;
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

        public static bool ContainsOne<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Count() == 1;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null || IsEmpty(enumerable))
            {
                return true;
            }

            return false;
        }

        public static System.Collections.IEnumerable ToGenericEnumerable(
            this System.Collections.IEnumerable enumerable, Type type)
        {
            var genericList = typeof(List<>).MakeGenericType(type);
            var list = Activator.CreateInstance(genericList) as System.Collections.IList;

            foreach (var item in enumerable)
            {
                list.Add(item);
            }

            return list;
        }

    }
}
