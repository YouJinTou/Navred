using Navred.Core.Tools;
using System;

namespace Navred.Core.Itineraries
{
    public class Stop
    {
        public Stop(string name, DateTime utcArrivalTime)
        {
            this.Name = Validator.ReturnOrThrowIfNullOrWhiteSpace(name);
            this.UtcArrivalTime = utcArrivalTime;
        }

        public string Name { get; }

        public DateTime UtcArrivalTime { get; }

        public override string ToString()
        {
            return $"{this.Name} - {this.UtcArrivalTime}";
        }
    }
}
