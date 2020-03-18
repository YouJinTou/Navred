using Navred.Core.Tools;
using System;

namespace Navred.Core.Itineraries
{
    public class Stop
    {
        public Stop(string name, DateTime arrivalTime)
        {
            this.Name = Validator.ReturnOrThrowIfNullOrWhiteSpace(name);
            this.ArrivalTime = arrivalTime;
        }

        public string Name { get; }

        public DateTime ArrivalTime { get; }

        public override string ToString()
        {
            return $"{this.Name} - {this.ArrivalTime}";
        }
    }
}
