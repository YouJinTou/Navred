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

        public static string ReplaceTokens(this string s, IEnumerable<string> replacements)
        {
            var tokens = s.Split(" ");
            var result = new List<string>();

            foreach (var t in tokens)
            {
                var current = t;

                foreach (var r in replacements)
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

            foreach (var r in replacements.OrderByDescending(r => r.Length))
            {
                result = result.Replace(r, string.Empty);
            }

            return result;
        }

        public static string ChainReplace(this string s, IDictionary<string, string> replacements)
        {
            var result = s;

            foreach (var kvp in replacements.OrderByDescending(r => r.Value.Length))
            {
                result = result.Replace(kvp.Key, kvp.Value, StringComparison.OrdinalIgnoreCase);
            }

            return result;
        }

        public static bool IsFuzzyMatch(this string a, string b)
        {
            var preprocessedA = a.ToLower().Replace("-", " ").Replace(".", string.Empty);
            preprocessedA = Regex.Replace(preprocessedA, "\\s+", " ");
            var preprocessedB = b.ToLower().Replace("-", " ").Replace(".", string.Empty);
            preprocessedB = Regex.Replace(preprocessedB, "\\s+", " ");
            var ratio = CalculateLevenshteinRatio(preprocessedA.ToLower(), preprocessedB.ToLower());
            var isFuzzyMatch = ratio >= 0.8d;

            return isFuzzyMatch;
        }

        // https://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
        // https://www.datacamp.com/community/tutorials/fuzzy-string-python
        public static double CalculateLevenshteinRatio(this string a, string b)
        {
            if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
            {
                return 0;
            }
            if (String.IsNullOrEmpty(a))
            {
                return b.Length;
            }
            if (String.IsNullOrEmpty(b))
            {
                return a.Length;
            }
            int lengthA = a.Length;
            int lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];
            for (int i = 0; i <= lengthA; distances[i, 0] = i++) ;
            for (int j = 0; j <= lengthB; distances[0, j] = j++) ;

            for (int i = 1; i <= lengthA; i++)
                for (int j = 1; j <= lengthB; j++)
                {
                    int cost = b[j - 1] == a[i - 1] ? 0 : 1;
                    distances[i, j] = Math.Min
                        (
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                        );
                }

            var ratio = (double)((lengthA + lengthB) - distances[lengthA, lengthB]) / (lengthA + lengthB);

            return ratio;
        }
    }
}
