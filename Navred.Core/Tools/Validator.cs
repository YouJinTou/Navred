using Navred.Core.Extensions;
using System;
using System.Collections.Generic;

namespace Navred.Core.Tools
{
    public static class Validator
    {
        public static void ThrowIfNull(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("Object is empty.");
            }

        }
        public static void ThrowIfNullOrWhiteSpace(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException("String is empty.");
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

        public static string ReturnOrThrowIfNullOrWhiteSpace(string s)
        {
            ThrowIfNullOrWhiteSpace(s);

            return s;
        }

        public static T ReturnOrThrowIfNull<T>(T item)
        {
            ThrowIfNull(item);

            return item;
        }

        public static void ThrowIfNullOrEmpty<T>(IEnumerable<T> enumerable)
        {
            if (enumerable.IsNullOrEmpty())
            {
                throw new ArgumentNullException("Enumerable is null or empty.");
            }
        }
    }
}
