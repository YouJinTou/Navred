﻿using Navred.Core.Itineraries;
using Navred.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using TimeZoneConverter;

namespace Navred.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static IEnumerable<DateTime> GetValidUtcTimesAhead(
            this DaysOfWeek dow,
            LegTime legTime,
            int daysAhead,
            IEnumerable<DateTime> holidays = null)
        {
            var includeHolidays = (dow & DaysOfWeek.HolidayInclusive) > 0;
            var excludeHolidays = (dow & DaysOfWeek.HolidayExclusive) > 0;

            if (includeHolidays && excludeHolidays)
            {
                throw new InvalidOperationException("Cannot include and exclude holidays.");
            }

            if ((includeHolidays || excludeHolidays) && holidays.IsNullOrEmpty())
            {
                throw new InvalidOperationException("No holidays provided.");
            }

            var offset = DateTimeOffset.Now;
            var firstDate = GetFirstAvailableUtcDate(dow);
            var times = new HashSet<DateTime>();
            holidays = holidays ?? new List<DateTime>();

            for (int d = 0; d < daysAhead; d++)
            {
                var currentDate = firstDate.AddDays(d);
                var time = currentDate.Date + legTime.Time - offset.Offset;

                if (excludeHolidays && holidays.Contains(currentDate))
                {
                    continue;
                }

                if (includeHolidays && holidays.Contains(currentDate))
                {
                    times.Add(time);
                }
                else if (currentDate.DayOfWeek.Matches(dow))
                {
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

        public static DateTimeTz ToUtcDateTimeTz(this DateTime dt)
        {
            return new DateTimeTz(dt, Constants.UtcTimeZone);
        }

        public static TimeSpan AddMinutes(this TimeSpan ts, int minutes)
        {
            return ts + TimeSpan.FromMinutes(minutes);
        }

        public static DateTime GetFirstAvailableUtcDate(this DaysOfWeek dow)
        {
            if (dow.Equals(DaysOfWeek.Empty))
            {
                throw new InvalidOperationException("Days of week is empty.");
            }

            var current = DateTime.UtcNow.Date;

            while (true)
            {
                if (current.DayOfWeek.Matches(dow))
                {
                    return current;
                }

                current = current.AddDays(1);
            }
        }

        public static DateTime ToUtcDateTimeDate(this TimeSpan ts, DateTime dt)
        {
            return dt.Date + ts - DateTimeOffset.Now.Offset;
        }

        // https://www.codeproject.com/Articles/10860/Calculating-Christian-Holidays
        public static DateTime ToOrthodoxEaster(this int year)
        {
            var easter = CalculateEaster(year, false);

            return easter;
        }

        private static DateTime CalculateEaster(int year, bool isCatholic)
        {
            // Gauss algorithm implementation

            // Calculate the difference between Julian and Gregorian calendars for the given year
            var century = year / 100;
            var gregorian_shift = century - century / 4 - 2;
            var x = 15;
            var y = 6;

            // Metonic cycle correction
            x += (2 - (13 + 8 * century) / 25 + gregorian_shift) & (isCatholic ? -1 : 0);
            y += gregorian_shift & (isCatholic ? -1 : 0);

            // The core formula
            var g = year % 19;
            var d = (g * 19 + x) % 30; // Paschal Full Moon
            var e = (2 * (year % 4) + 4 * year - d + y) % 7; // Sunday after PFM

            var day = d + e;

            // Correction for the length of the moon month
            day -= (e == 6) & ((d == 29) | ((d == 28) & (g > 10))) ? 7 : 0;

            // Convert Orthodox Easter to Grigorian calendar
            day += gregorian_shift & (isCatholic ? 0 : -1);

            // Calculate month and day
            var is_may = (day >= 40);
            var is_not_march = (day >= 10);

            var month = 3 + (is_not_march ? 1 : 0) + (is_may ? 1 : 0);
            day += 22 - (is_not_march ? 31 : 0) - (is_may ? 30 : 0);

            //Your Easter date is in 'year', 'month', 'day'
            return new DateTime(year, month, day);
        }
    }
}
