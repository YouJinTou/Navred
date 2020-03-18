using Navred.Core.Itineraries;
using System;
using System.Collections.Generic;

namespace Navred.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static IEnumerable<DateTime> GetValidUtcTimesAhead(
            this DaysOfWeek daysOfWeek, StopTime time, int daysAhead)
        {
            var utc = DateTime.UtcNow;
            var dates = new List<DateTime>();

            for (int d = 0; d < daysAhead; d++)
            {
                var currentDate = utc.AddDays(d);

                if (currentDate.DayOfWeek.Matches(daysOfWeek))
                {
                    var date = currentDate.Date + time.Time;

                    dates.Add(date);
                }
            }

            return dates;
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
    }
}
