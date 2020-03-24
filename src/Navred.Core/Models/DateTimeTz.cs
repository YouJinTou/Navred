using Navred.Core.Tools;
using System;

namespace Navred.Core.Models
{
    public class DateTimeTz
    {
        public DateTimeTz(DateTime dt, string timeZone)
        {
            TimeZoneInfo.FindSystemTimeZoneById(timeZone);

            this.DateTime = dt;
            this.TimeZone = Validator.ReturnOrThrowIfNullOrWhiteSpace(timeZone);
        }

        public DateTime DateTime { get; }

        public string TimeZone { get; }

        public static bool operator <=(DateTimeTz x, DateTimeTz y)
        {
            return x.DateTime <= y.DateTime;
        }

        public static bool operator >=(DateTimeTz x, DateTimeTz y)
        {
            return x.DateTime >= y.DateTime;
        }

        public static DateTimeTz operator +(DateTimeTz left, TimeSpan right)
        {
            return new DateTimeTz(left.DateTime + right, left.TimeZone);
        }

        public override string ToString()
        {
            return $"{this.DateTime.ToString()} {this.TimeZone}";
        }
    }
}
