﻿using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using System;
using System.Text.RegularExpressions;

namespace Navred.Core.Itineraries
{
    public class LegTime : IEquatable<LegTime>, ICopyable<LegTime>
    {
        private const string Pattern = @"(\d{1,2})([:.,])(\d{1,2})";

        public static readonly LegTime Estimable = new LegTime
        {
            Estimated = true,
            Time = DateTime.MaxValue - new DateTime(3000, 1, 1, 0, 0, 0)
        };

        private LegTime()
        {
        }

        public LegTime(string time, bool estimated = false)
        {
            this.Time = this.TimeToTimeSpan(time.ReturnOrThrowIfNullOrWhiteSpace());
            this.Estimated = estimated;
        }

        public TimeSpan Time { get; private set; }

        public bool Estimated { get; set; }

        public static implicit operator LegTime(string time)
        {
            return new LegTime(time);
        }

        public static implicit operator LegTime(TimeSpan timeSpan)
        {
            return new LegTime(timeSpan.ToString());
        }

        public static bool operator <(LegTime left, LegTime right)
        {
            return left.Time < right.Time;
        }

        public static bool operator >(LegTime left, LegTime right)
        {
            return left.Time > right.Time;
        }

        public static bool IsValid(string s)
        {
            var formatted = s.Trim();
            var isValid = Regex.IsMatch(formatted, Pattern);

            return isValid;
        }

        public override string ToString()
        {
            return this.Time.ToString();
        }

        public bool Equals(LegTime other)
        {
            return (other == null) ? false : this.Time.Equals(other.Time);
        }

        public LegTime Copy()
        {
            return new LegTime
            {
                Estimated = this.Estimated,
                Time = this.Time
            };
        }

        private TimeSpan TimeToTimeSpan(string time)
        {
            var formattedTime = time.Trim();

            if (!IsValid(formattedTime))
            {
                throw new ArgumentException(
                    $"{nameof(time)} must be in the dd:dd format, where 'd' is a digit.");
            }

            var match = Regex.Match(formattedTime, Pattern);
            var hours = int.Parse(match.Groups[1].Value);

            if (hours >= 24)
            {
                throw new ArgumentException("Hours must be up to 23, inclusive.");
            }

            var minutes = int.Parse(match.Groups[3].Value);

            if (minutes >= 60)
            {
                throw new ArgumentException("Minutes must be up to 59, inclusive.");
            }

            var hoursTimeSpan = TimeSpan.FromHours(hours);
            var minutesTimeSpan = TimeSpan.FromMinutes(minutes);
            var span = hoursTimeSpan + minutesTimeSpan;

            return span;
        }
    }
}
