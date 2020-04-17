using Navred.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Tools
{
    public static class Validator
    {
        public static void ThrowIfAnyNull(params object[] objects)
        {
            foreach (var o in objects)
            {
                o.ThrowIfNull();
            }
        }

        public static void ThrowIfAnyNullOrWhiteSpace(params object[] objects)
        {
            foreach (var o in objects)
            {
                if (o is string)
                {
                    (o as string).ThrowIfNullOrWhiteSpace();
                }
                else
                {
                    o.ThrowIfNull();
                }
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

        public static bool AnyNullOrWhiteSpace(params object[] objects)
        {
            foreach (var item in objects)
            {
                if (item == null)
                {
                    return true;
                }

                if (item is string && string.IsNullOrWhiteSpace(item as string))
                {
                    return true;
                }
            }

            return false;
        }

        public static void ThrowIfAllNull<T>(IEnumerable<T> enumerable, string message = null)
        {
            enumerable.ThrowIfNull(message);

            if (enumerable.All(i => i == null))
            {
                throw new ArgumentNullException(message ?? "All null.");
            }
        }
    }
}
