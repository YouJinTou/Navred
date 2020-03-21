using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;

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
            var normalizedPlace = place.Replace(" ", "").ToLower();
            var matches = places
                .Where(p => normalizedPlace.Contains(p.Place.Replace(" ", "").ToLower())).ToList();
            var match = this.GetNormalizedPlaceName(matches, discerningCode);

            if (string.IsNullOrWhiteSpace(match))
            {
                matches = places
                    .Where(p => p.Place.Replace(" ", "").ToLower().Contains(normalizedPlace))
                    .ToList();
                match = this.GetNormalizedPlaceName(matches, discerningCode);
            }

            return string.IsNullOrWhiteSpace(match) ?
                throw new KeyNotFoundException($"Could not find '{place}' in Bulgaria.") :
                match;
        }

        private string GetNormalizedPlaceName(List<BulgarianPlace> places, string areaCode)
        {
            if (places.Count == 1)
            {
                return places.First().Place;
            }

            if (places.Count > 1)
            {
                return places.FirstOrDefault(m => m.AreaCode == areaCode)?.Place;
            }

            return null;
        }
    }
}
