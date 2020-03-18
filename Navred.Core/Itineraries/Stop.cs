using Navred.Core.Tools;
using System;
using System.Text.RegularExpressions;

namespace Navred.Core.Itineraries
{
    public class Stop
    {
        public Stop(string name, string arrivalTime)
        {
            this.Name = Validator.ReturnOrThrowIfNullOrWhiteSpace(name);
            this.ArrivalTime = this.ArrivalTimeToTimeSpan(arrivalTime);
        }

        public string Name { get; }

        public TimeSpan ArrivalTime { get; }

        public override string ToString()
        {
            return $"{this.Name} - {this.ArrivalTime}";
        }

        private TimeSpan ArrivalTimeToTimeSpan(string arrivalTime)
        {
            if (!Regex.IsMatch(arrivalTime, @"\d\d:\d\d"))
            {
                throw new ArgumentException(
                    $"{nameof(arrivalTime)} must be in the dd:dd format, where 'd' is a digit.");
            }

            var hours = int.Parse(arrivalTime.Split(':')[0]);

            if (hours >= 24)
            {
                throw new ArgumentException("Hours must be up to 23, inclusive.");
            }

            var minutes = int.Parse(arrivalTime.Split(':')[1]);

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
