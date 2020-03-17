using System;

namespace Navred.Core.Models
{
    public class Stop
    {
        public Stop(string name, string arrivalTime)
        {
            this.Name = name;
            this.ArrivalTime = this.ArrivalTimeToTimeSpan(arrivalTime);
        }

        public string Name { get; set; }

        public TimeSpan ArrivalTime { get; set; }

        public override string ToString()
        {
            return $"{this.Name} - {this.ArrivalTime}";
        }

        private TimeSpan ArrivalTimeToTimeSpan(string arrivalTime)
        {
            var hours = int.Parse(arrivalTime.Split(':')[0]);
            var minutes = int.Parse(arrivalTime.Split(':')[1]);
            var hoursTimeSpan = TimeSpan.FromHours(hours);
            var minutesTimeSpan = TimeSpan.FromMinutes(minutes);
            var span = hoursTimeSpan + minutesTimeSpan;

            return span;
        }
    }
}
