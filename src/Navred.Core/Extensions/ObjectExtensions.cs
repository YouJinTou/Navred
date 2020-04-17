using System;

namespace Navred.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object obj)
        {
            return obj == null;
        }

        public static T ReturnBigger<T>(this T left, T right) where T : IComparable<T>
        {
            var result = left.CompareTo(right);

            return (result > 0) ? left : right;
        }
    }
}
