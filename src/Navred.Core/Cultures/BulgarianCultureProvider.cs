using System.Collections.Generic;

namespace Navred.Core.Cultures
{
    public class BulgarianCultureProvider : IBulgarianCultureProvider
    {
        public static string AllLetters = "ѝабвгдежзийклмнопрстуфхцчшщъьыюяАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЮЯ" + "ь".ToUpper() + "ы".ToUpper();
        public const string CountryName = "Bulgaria";

        public class Region
        {
            public const string BLG = "Благоевград";
            public const string BGS = "Бургас";
            public const string VAR = "Варна";
            public const string VTR = "Велико Търново";
            public const string VID = "Видин";
            public const string VRC = "Враца";
            public const string GAB = "Габрово";
            public const string DOB = "Добрич";
            public const string KRZ = "Кърджали";
            public const string KNL = "Кюстендил";
            public const string LOV = "Ловеч";
            public const string MON = "Монтана";
            public const string PAZ = "Пазарджик";
            public const string PER = "Перник";
            public const string PVN = "Плевен";
            public const string PDV = "Пловдив";
            public const string RAZ = "Разград";
            public const string RSE = "Русе";
            public const string SLS = "Силистра";
            public const string SLV = "Сливен";
            public const string SML = "Смолян";
            public const string SFO = "София";
            public const string SOF = "София (столица)";
            public const string SZR = "Стара Загора";
            public const string TGV = "Търговище";
            public const string HKV = "Хасково";
            public const string SHU = "Шумен";
            public const string JAM = "Ямбол";

            public static IReadOnlyDictionary<string, string> NameByCode => new Dictionary<string, string>
            {
                { nameof(BLG), BLG },
                { nameof(BGS), BGS },
                { nameof(VAR), VAR },
                { nameof(VTR), VTR },
                { nameof(VID), VID },
                { nameof(VRC), VRC },
                { nameof(GAB), GAB },
                { nameof(DOB), DOB },
                { nameof(KRZ), KRZ },
                { nameof(KNL), KNL },
                { nameof(LOV), LOV },
                { nameof(MON), MON },
                { nameof(PAZ), PAZ },
                { nameof(PER), PER },
                { nameof(PVN), PVN },
                { nameof(PDV), PDV },
                { nameof(RAZ), RAZ },
                { nameof(RSE), RSE },
                { nameof(SLS), SLS },
                { nameof(SLV), SLV },
                { nameof(SML), SML },
                { nameof(SFO), SFO },
                { nameof(SOF), SOF },
                { nameof(SZR), SZR },
                { nameof(TGV), TGV },
                { nameof(HKV), HKV },
                { nameof(SHU), SHU },
                { nameof(JAM), JAM }
            };
        }

        public string Name => CountryName;

        public string Letters => AllLetters;

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
    }
}
