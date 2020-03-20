using Navred.Core.Tools;
using System;

namespace Navred.Core.Itineraries
{
    public class Stop
    {
        public Stop()
        {
        }

        public Stop(string name, DateTime utcArrivalTime, string carrier, decimal? price = null)
        {
            this.Name = Validator.ReturnOrThrowIfNullOrWhiteSpace(name);
            this.UtcArrivalTime = utcArrivalTime;
            this.Carrier = Validator.ReturnOrThrowIfNullOrWhiteSpace(carrier);
            this.Price = price;
        }

        public string Name { get; private set; }

        public DateTime UtcArrivalTime { get; private set; }

        public string Carrier { get; private set; }

        public decimal? Price { get; private set; }

        public override string ToString()
        {
            return $"{this.Name} - {this.UtcArrivalTime} | {this.Carrier} {this.Price}";
        }
    }
}
