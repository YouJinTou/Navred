using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Navred.Core.Cultures
{
    public class BulgarianCultureProvider : IBulgarianCultureProvider
    {
        public static string AllLetters = "ѝабвгдежзийклмнопрстуфхцчшщъьыюяАБВГДЕЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЮЯ" + "ь".ToUpper() + "ы".ToUpper();
        public const string CountryName = "Bulgaria";

        public static class Region
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

        public static class City
        {
            public const string Sofia = "София";
            public const string VelikoTarnovo = "Велико Търново";
            public const string Plovdiv = "Пловдив";
            public const string Hisarya = "Хисаря";
            public const string Lovech = "Ловеч";
            public const string Bourgas = "Бургас";
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
                        buffer.Add(char.IsUpper(ch) ? "Ts" : "ts");
                        break;
                    case 'ч':
                        buffer.Add(char.IsUpper(ch) ? "Tsch" : "tsch");
                        break;
                    case 'ш':
                        buffer.Add(char.IsUpper(ch) ? "Sh" : "sh");
                        break;
                    case 'щ':
                        buffer.Add(char.IsUpper(ch) ? "Sht" : "sht");
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
                        buffer.Add(char.IsUpper(ch) ? "Yy" : "yu");
                        break;
                    case 'я':
                        buffer.Add(char.IsUpper(ch) ? "Ya" : "ya");
                        break;
                    default:
                        buffer.Add(ch.ToString());
                        break;
                }
            }

            var result = string.Join(string.Empty, buffer);

            return result;
        }

        public DaysOfWeek ToDaysOfWeek(string dayString)
        {
            dayString.ThrowIfNullOrWhiteSpace("Empty day string.");

            var d = dayString.Trim().ToLower();

            switch (d)
            {
                case var _ when d.Equals("нд") || d.Equals("неделя"): return DaysOfWeek.Sunday;
                case var _ when d.Equals("пн") || d.Equals("понеделник"): return DaysOfWeek.Monday;
                case var _ when d.Equals("вт") || d.Equals("вторник"): return DaysOfWeek.Tuesday;
                case var _ when d.Equals("ср") || d.Equals("сряда"): return DaysOfWeek.Wednesday;
                case var _ when d.Equals("чт") || d.Equals("четвъртък"): return DaysOfWeek.Thursday;
                case var _ when d.Equals("пт") || d.Equals("пк") || d.Equals("петък"): return DaysOfWeek.Friday;
                case var _ when d.Equals("сб") || d.Equals("събота"): return DaysOfWeek.Saturday;
                default: throw new KeyNotFoundException($"Could not map {dayString}.");
            }
        }

        public DaysOfWeek ToDaysOfWeek(IEnumerable<string> dayStrings)
        {
            DaysOfWeek days = 0;

            foreach (var dayString in dayStrings)
            {
                days |= this.ToDaysOfWeek(dayString);
            }

            return days;
        }

        public IEnumerable<DateTime> GetHolidays(uint yearsAhead = 2)
        {
            var dates = new List<DateTime>();
            var date = DateTime.UtcNow;

            for (int i = 0; i < yearsAhead; i++)
            {
                var d = date.AddYears(i);
                var newYears = new DateTime(d.Year, 1, 1);
                var liberationDay = new DateTime(d.Year, 3, 3);
                var easter = d.Year.ToOrthodoxEaster();
                var laborDay = new DateTime(d.Year, 5, 1);
                var armyDay = new DateTime(d.Year, 5, 6);
                var cultureDay = new DateTime(d.Year, 5, 24);
                var unionDay = new DateTime(d.Year, 9, 6);
                var independenceDay = new DateTime(d.Year, 9, 22);
                var christmasEve = new DateTime(d.Year, 12, 24);
                var christmasDay = new DateTime(d.Year, 12, 25);
                var christmasDayAfter = new DateTime(d.Year, 12, 26);

                dates.Add(newYears);
                dates.Add(liberationDay);
                dates.Add(easter);
                dates.Add(laborDay);
                dates.Add(armyDay);
                dates.Add(cultureDay);
                dates.Add(unionDay);
                dates.Add(independenceDay);
                dates.Add(christmasEve);
                dates.Add(christmasDay);
                dates.Add(christmasDayAfter);
            }

            return dates;
        }

        public Encoding GetEncoding()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            return Encoding.GetEncoding("windows-1251");
        }
    }
}
