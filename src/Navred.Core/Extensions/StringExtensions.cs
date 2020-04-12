using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Navred.Core.Extensions
{
    public static class StringExtensions
    {
        public static decimal? StripCurrency(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            var price = decimal.Parse(Regex.Match(s, @"(\d+[\.,]?\d*)").Groups[1].Value);

            return price;
        }

        public static string FromUnicode(this string s)
        {
            var result = Regex.Replace(
                s, 
                @"(?i)\\[uU]([0-9a-f]{4})", 
                m => ((char)Convert.ToInt32(m.Groups[1].Value, 16)).ToString());

            return result;
        }

        public static string ReplaceTokens(this string s, IEnumerable<string> replaces)
        {
            var tokens = s.Split(" ");
            var result = new List<string>();

            foreach (var t in tokens)
            {
                var current = t;

                foreach (var r in replaces)
                {
                    current = current.Replace(r, string.Empty);
                }

                result.Add(current);
            }

            var resultString = string.Join(" ", result.Where(x => !string.IsNullOrWhiteSpace(x)));

            return resultString;
        }

        public static string ChainReplace(this string s, IEnumerable<string> replacements)
        {
            var result = s;

            foreach (var r in replacements)
            {
                result = result.Replace(r, string.Empty);
            }

            return result;
        }

        public static string ChainReplace(this string s, IDictionary<string, string> replacements)
        {
            var result = s;

            foreach (var kvp in replacements)
            {
                result = result.Replace(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }
    }
}
