using Navred.Core.Itineraries;
using System;
using System.Collections.Generic;

namespace Navred.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static IEnumerable<DateTime> GetValidUtcTimesAhead(
            this DaysOfWeek daysOfWeek, StopTime stopTime, int daysAhead)
        {
            var offset = DateTimeOffset.Now;
            var times = new List<DateTime>();

            for (int d = 0; d < daysAhead; d++)
            {
                var currentDate = offset.UtcDateTime.AddDays(d);

                if (currentDate.DayOfWeek.Matches(daysOfWeek))
                {
                    var time = currentDate.Date + stopTime.Time - offset.Offset;

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

        public static long ToUtcTimestamp(this DateTime dt)
        {
            var utcDt = TimeZoneInfo.ConvertTimeToUtc(dt);
            var utcSpan = utcDt - DateTimeOffset.UnixEpoch;
            var timestmap = (long)utcSpan.TotalSeconds;

            return timestmap;
        }

        public static DateTime ToUtcDateTime(this long utcTimestamp)
        {
            var utcDt = DateTimeOffset.FromUnixTimeSeconds(utcTimestamp).UtcDateTime;

            return utcDt;
        }
    }
}
