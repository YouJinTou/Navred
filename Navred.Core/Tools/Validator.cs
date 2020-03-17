using System;

namespace Navred.Core.Tools
{
    public static class Validator
    {
        public static void ThrowIfNullOrWhiteSpace(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentNullException("String is empty.");
            }
        }

        public static void ThrowIfAnyNullOrWhiteSpace(params string[] strings)
        {
            foreach (var s in strings)
            {
                ThrowIfNullOrWhiteSpace(s);
            }
        }
    }
}
