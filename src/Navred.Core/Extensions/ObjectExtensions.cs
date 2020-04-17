using System;
using System.Collections.Generic;

namespace Navred.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static void ThrowIfNull(this object obj, string message = null)
        {
            if (IsNull(obj))
            {
                throw new ArgumentNullException(message ?? "Object is empty.");
            }

        }

        public static void ThrowIfNullOrWhiteSpace(this string s, string message = null)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException(message ?? "String is empty.");
            }
        }

        public static string ReturnOrThrowIfNullOrWhiteSpace(this string s, string message = null)
        {
            ThrowIfNullOrWhiteSpace(s, message);

            return s;
        }

        public static T ReturnOrThrowIfNull<T>(this T item, string message = null)
        {
            ThrowIfNull(item, message);

            return item;
        }

        public static IEnumerable<T> ReturnOrThrowIfNullOrEmpty<T>(
            this IEnumerable<T> item, string message = null)
        {
            ThrowIfNullOrEmpty(item, message);

            return item;
        }

        public static void ThrowIfNullOrEmpty<T>(
            this IEnumerable<T> enumerable, string message = null)
        {
            if (enumerable.IsNullOrEmpty())
            {
                throw new ArgumentNullException(message ?? "Enumerable is null or empty.");
            }
        }

        public static T ReturnBigger<T>(this T left, T right) where T : IComparable<T>
        {
            var result = left.CompareTo(right);

            return (result > 0) ? left : right;
        }
    }
}
