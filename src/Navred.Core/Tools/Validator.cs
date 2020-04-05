using Navred.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Tools
{
    public static class Validator
    {
        public static void ThrowIfNull(object obj, string message = null)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(message ?? "Object is empty.");
            }

        }
        public static void ThrowIfNullOrWhiteSpace(string s, string message = null)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException(message ?? "String is empty.");
            }
        }

        public static void ThrowIfAnyNullOrWhiteSpace(params object[] objects)
        {
            foreach (var o in objects)
            {
                if (o is string)
                {
                    ThrowIfNullOrWhiteSpace(o as string);
                }
                else
                {
                    ThrowIfNull(o);
                }
            }
        }

        public static string ReturnOrThrowIfNullOrWhiteSpace(string s, string message = null)
        {
            ThrowIfNullOrWhiteSpace(s, message);

            return s;
        }

        public static T ReturnOrThrowIfNull<T>(T item, string message = null)
        {
            ThrowIfNull(item, message);

            return item;
        }

        public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> enumerable, string message = null)
        {
            if (enumerable.IsNullOrEmpty())
            {
                throw new ArgumentNullException(message ?? "Enumerable is null or empty.");
            }
        }

        public static bool AnyNull(System.Collections.IEnumerable enumerable)
        {
            foreach (var item in enumerable)
            {
                if (item == null)
                {
                    return true;
                }
            }

            return false;
        }

        public static void ThrowIfAllNull<T>(IEnumerable<T> enumerable, string message = null)
        {
            ThrowIfNull(enumerable, message);

            if (enumerable.All(i => i == null))
            {
                throw new ArgumentNullException(message ?? "All null.");
            }
        }

        public static bool AllNullOrWhiteSpace(params string[] strings)
        {
            return strings.All(s => string.IsNullOrWhiteSpace(s));
        }
    }
}
