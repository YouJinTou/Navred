using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Navred.Core.Cultures
{
    public class BulgarianCultureProvider : IBulgarianCultureProvider
    {
        public static string Letters = "ѝабвгдежзийклмнопрстуфхцчшщъьыюяАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЮЯ" + "ь".ToUpper() + "ы".ToUpper();

        public string Name => "Bulgaria";

        public string Latinize(string s)
        {
            var buffer = new List<string>();

            foreach (var ch in s)
            {
                var lower = ch.ToString().ToLower()[0];

                switch (lower)
                {
                    case 'ѝ':
                        buffer.Add(char.IsUpper(ch) ? "I" : "i");
                        break;
                    case 'а':
                        buffer.Add(char.IsUpper(ch) ? "A" : "a");
                        break;
                    case 'б':
                        buffer.Add(char.IsUpper(ch) ? "B" : "b");
                        break;
                    case 'в':
                        buffer.Add(char.IsUpper(ch) ? "V" : "v");
                        break;
                    case 'г':
                        buffer.Add(char.IsUpper(ch) ? "G" : "g");
                        break;
                    case 'д':
                        buffer.Add(char.IsUpper(ch) ? "D" : "d");
                        break;
                    case 'е':
                        buffer.Add(char.IsUpper(ch) ? "E" : "e");
                        break;
                    case 'ж':
                        buffer.Add(char.IsUpper(ch) ? "ZH" : "zh");
                        break;
                    case 'з':
                        buffer.Add(char.IsUpper(ch) ? "Z" : "z");
                        break;
                    case 'и':
                        buffer.Add(char.IsUpper(ch) ? "I" : "i");
                        break;
                    case 'й':
                        buffer.Add(char.IsUpper(ch) ? "Y" : "y");
                        break;
                    case 'к':
                        buffer.Add(char.IsUpper(ch) ? "K" : "k");
                        break;
                    case 'л':
                        buffer.Add(char.IsUpper(ch) ? "L" : "l");
                        break;
                    case 'м':
                        buffer.Add(char.IsUpper(ch) ? "M" : "m");
                        break;
                    case 'н':
                        buffer.Add(char.IsUpper(ch) ? "N" : "n");
                        break;
                    case 'о':
                        buffer.Add(char.IsUpper(ch) ? "O" : "o");
                        break;
                    case 'п':
                        buffer.Add(char.IsUpper(ch) ? "P" : "p");
                        break;
                    case 'р':
                        buffer.Add(char.IsUpper(ch) ? "R" : "r");
                        break;
                    case 'с':
                        buffer.Add(char.IsUpper(ch) ? "S" : "s");
                        break;
                    case 'т':
                        buffer.Add(char.IsUpper(ch) ? "T" : "t");
                        break;
                    case 'у':
                        buffer.Add(char.IsUpper(ch) ? "U" : "u");
                        break;
                    case 'ф':
                        buffer.Add(char.IsUpper(ch) ? "F" : "f");
                        break;
                    case 'х':
                        buffer.Add(char.IsUpper(ch) ? "H" : "h");
                        break;
                    case 'ц':
                        buffer.Add(char.IsUpper(ch) ? "TS" : "ts");
                        break;
                    case 'ч':
                        buffer.Add(char.IsUpper(ch) ? "TSCH" : "tsch");
                        break;
                    case 'ш':
                        buffer.Add(char.IsUpper(ch) ? "SH" : "sh");
                        break;
                    case 'щ':
                        buffer.Add(char.IsUpper(ch) ? "SHT" : "sht");
                        break;
                    case 'ъ':
                        buffer.Add(char.IsUpper(ch) ? "A" : "a");
                        break;
                    case 'ь':
                        buffer.Add(char.IsUpper(ch) ? "Y" : "y");
                        break;
                    case 'ы':
                        buffer.Add(char.IsUpper(ch) ? "Y" : "y");
                        break;
                    case 'ю':
                        buffer.Add(char.IsUpper(ch) ? "YU" : "yu");
                        break;
                    case 'я':
                        buffer.Add(char.IsUpper(ch) ? "YA" : "ya");
                        break;
                    default:
                        buffer.Add(ch.ToString());
                        break;
                }
            }

            var result = string.Join(string.Empty, buffer);

            return result;
        }

        public string NormalizePlaceName(string place, string discerningCode = null)
        {
            var places = PlacesLoader.LoadPlacesFor<BulgarianPlace>(this.Name);
            var normalizedPlace = place.Replace(" ", "").Trim().ToLower();
            var matches = places
                .Where(p => normalizedPlace.Contains(p.Name.Replace(" ", "").ToLower())).ToList();
            var match = this.GetNormalizedPlaceName(matches, discerningCode);

            if (string.IsNullOrWhiteSpace(match))
            {
                matches = places
                    .Where(p => p.Name.Replace(" ", "").ToLower().Contains(normalizedPlace))
                    .ToList();
                match = this.GetNormalizedPlaceName(matches, discerningCode);
            }

            match = (match == null) ? this.DoFuzzyMatch(normalizedPlace) : match;

            return string.IsNullOrWhiteSpace(match) ?
                throw new KeyNotFoundException($"Could not find '{place}' in Bulgaria.") :
                match;
        }

        private string GetNormalizedPlaceName(List<BulgarianPlace> places, string areaCode)
        {
            if (places.Count == 1)
            {
                return places.First().Name;
            }

            if (places.Count > 1)
            {
                return places.FirstOrDefault(m => m.AreaCode == areaCode)?.Name;
            }

            return null;
        }

        private string DoFuzzyMatch(string normalizedPlace)
        {
            var separators = new char[] { '.', '-', ' ' };

            foreach (var separator in separators)
            {
                var tokens = normalizedPlace.Split(separator);

                if (tokens.Length <= 1)
                {
                    continue;
                }

                var places = PlacesLoader.LoadPlacesFor<BulgarianPlace>(this.Name);

                foreach (var p in places)
                {
                    foreach (var sep in separators)
                    {
                        var storedPlaceTokens = p.Name.Split(sep);

                        if (storedPlaceTokens.Length == tokens.Length)
                        {
                            var allMatch = true;

                            for (int t = 0; t < tokens.Length; t++)
                            {
                                var tokenMatches =
                                    storedPlaceTokens[t].ToLower().Trim().Contains(tokens[t]);
                                allMatch = allMatch && tokenMatches;
                            }

                            if (allMatch)
                            {
                                return p.Name;
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
