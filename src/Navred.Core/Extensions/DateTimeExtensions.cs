using Navred.Core.Itineraries;
using System;
using System.Collections.Generic;
using TimeZoneConverter;

namespace Navred.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static IEnumerable<DateTime> GetValidUtcTimesAhead(
            this DaysOfWeek daysOfWeek, LegTime legTime, int daysAhead)
        {
            var offset = DateTimeOffset.Now;
            var times = new List<DateTime>();

            for (int d = 0; d < daysAhead; d++)
            {
                var currentDate = offset.UtcDateTime.AddDays(d);

                if (currentDate.DayOfWeek.Matches(daysOfWeek))
                {
                    var time = currentDate.Date + legTime.Time - offset.Offset;

                    times.Add(time);
                }
            }

            return times;
        }

        public static bool Matches(this DayOfWeek dayOfWeek, DaysOfWeek daysOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Friday:
                    return (daysOfWeek & DaysOfWeek.Friday) > 0;
                case DayOfWeek.Monday:
                    return (daysOfWeek & DaysOfWeek.Monday) > 0;
                case DayOfWeek.Saturday:
                    return (daysOfWeek & DaysOfWeek.Saturday) > 0;
                case DayOfWeek.Sunday:
                    return (daysOfWeek & DaysOfWeek.Sunday) > 0;
                case DayOfWeek.Thursday:
                    return (daysOfWeek & DaysOfWeek.Thursday) > 0;
                case DayOfWeek.Tuesday:
                    return (daysOfWeek & DaysOfWeek.Tuesday) > 0;
                case DayOfWeek.Wednesday:
                    return (daysOfWeek & DaysOfWeek.Wednesday) > 0;
                default:
                    throw new ArgumentException($"{dayOfWeek} invalid.");
            }
        }

        public static long ToUtcTimestamp(this DateTime utcDt)
        {
            var realUtcDt = DateTime.SpecifyKind(utcDt, DateTimeKind.Utc);
            var utcSpan = realUtcDt - DateTimeOffset.UnixEpoch;
            var timestamp = (long)utcSpan.TotalSeconds;

            return timestamp;
        }

        public static long ToUtcTimestamp(this DateTime dt, string fromTimeZone)
        {
            var sourceTimeZone = TZConvert.GetTimeZoneInfo(fromTimeZone);
            var utcDt = TimeZoneInfo.ConvertTimeToUtc(dt, sourceTimeZone);
            var utcSpan = utcDt - DateTimeOffset.UnixEpoch;
            var timestmap = (long)utcSpan.TotalSeconds;

            return timestmap;
        }

        public static DateTime ToUtcDateTime(this long utcTimestamp)
        {
            var utcDt = DateTimeOffset.FromUnixTimeSeconds(utcTimestamp).UtcDateTime;

            return utcDt;
        }

        public static DateTime ToUtcDateTime(this DateTime dt, string fromTimeZone)
        {
            var unspecified = DateTime.SpecifyKind(dt, DateTimeKind.Unspecified);
            var sourceTimeZone = TZConvert.GetTimeZoneInfo(fromTimeZone);
            var utcDt = TimeZoneInfo.ConvertTimeToUtc(unspecified, sourceTimeZone);

            return utcDt;
        }

        public static IEnumerable<DateTime> GetDateTimesAhead(
            this DateTime dt, int daysAhead, bool currentInclusive = false)
        {
            var dates = new List<DateTime>();

            for (
                int d = currentInclusive ? 0 : 1; 
                d < (currentInclusive ? daysAhead : daysAhead + 1); 
                d++)
            {
                dates.Add(dt.AddDays(d));
            }

            return dates;
        }
    }
}
