using System;

namespace Navred.Core.Models
{
    public class Stop
    {
        public string Name { get; set; }

        public string ArrivalTime { get; set; }

        public TimeSpan ArrivalTimeToTimeSpan()
        {
            var hours = int.Parse(this.ArrivalTime.Split(':')[0]);
            var minutes = int.Parse(this.ArrivalTime.Split(':')[1]);
            var hoursTimeSpan = TimeSpan.FromHours(hours);
            var minutesTimeSpan = TimeSpan.FromMinutes(minutes);
            var span = hoursTimeSpan + minutesTimeSpan;

            return span;
        }

        public override string ToString()
        {
            return $"{this.Name} - {this.ArrivalTime}";
        }
    }
}
