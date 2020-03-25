using System;
using System.Text.RegularExpressions;

namespace Navred.Core.Itineraries
{
    public class LegTime
    {
        public LegTime(string time)
        {
            this.Time = this.TimeToTimeSpan(time);
        }

        public TimeSpan Time { get; }

        public static implicit operator LegTime(string time)
        {
            return new LegTime(time);
        }

        private TimeSpan TimeToTimeSpan(string time)
        {
            var formattedTime = time.Trim();

            if (!Regex.IsMatch(formattedTime, @"\d\d:\d\d"))
            {
                throw new ArgumentException(
                    $"{nameof(time)} must be in the dd:dd format, where 'd' is a digit.");
            }

            var hours = int.Parse(formattedTime.Split(':')[0]);

            if (hours >= 24)
            {
                throw new ArgumentException("Hours must be up to 23, inclusive.");
            }

            var minutes = int.Parse(formattedTime.Split(':')[1]);

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
