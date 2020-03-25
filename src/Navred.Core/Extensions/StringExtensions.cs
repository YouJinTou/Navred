using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Navred.Core.Extensions
{
    public static class StringExtensions
    {
        public static bool IsFuzzyMatch(this string s, string other)
        {
            var separators = new string[] { ".", "-", " ", "," };
            var otherTokens = other.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var sep in separators)
            {
                var tokens = s.ToLower().Trim().Split(sep);

                if (tokens.Length != otherTokens.Length)
                {
                    continue;
                }

                var allMatch = true;

                for (int t = 0; t < otherTokens.Length; t++)
                {
                    var tokenMatches = tokens[t].ToLower().Trim().Contains(otherTokens[t]);
                    allMatch = allMatch && tokenMatches;
                }

                if (allMatch)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool SubstringMatchesPartially(
            this string s, string other, double threshold = 0.6)
        {
            var source = Regex.Replace(s.Trim().ToLower(), "[().,]", "");
            var target = Regex.Replace(other.Trim().ToLower(), "[().,]", "");
            var sourceTokens = source.Split(
                new string[] { " ", "-" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var targetTokens = target.Split(
                new string[] { " ", "-" }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var matchMatrix = new double[sourceTokens.Length, targetTokens.Length];

            for (int i = 0; i < sourceTokens.Length; i++)
            {
                var st = sourceTokens[i];

                for (int j = 0; j < targetTokens.Length; j++)
                {
                    var matches = 0;
                    var tt = targetTokens[j];
                    var sourceTokenShorter = (st.Length < tt.Length);
                    var limit = sourceTokenShorter ? st.Length : tt.Length;

                    for (int c = 0; c < limit; c++)
                    {
                        matches = (st[c] == tt[c]) ? matches + 1 : matches;

                        if (st[c] != tt[c])
                        {
                            break;
                        }
                    }

                    var sourceShorter = (st.Length < tt.Length);
                    var matchPercentage = (double)matches / (sourceShorter ? tt : st).Length;
                    matchMatrix[i,j] = matchPercentage;
                }
            }

            var percentage = matchMatrix.GetMatrixSum() / targetTokens.Length;
            
            return percentage > threshold;
        }

        public static decimal? StripCurrency(this string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                return null;
            }

            var price = decimal.Parse(Regex.Match(s, @"(\d+[\.,]?\d*)").Groups[1].Value);

            return price;
        }
    }
}
