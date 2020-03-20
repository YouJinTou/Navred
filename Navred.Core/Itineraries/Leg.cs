using Navred.Core.Tools;
using System;

namespace Navred.Core.Itineraries
{
    public class Leg
    {
        public Leg()
        {
        }

        public Leg(
            string from, 
            string to, 
            DateTime utcDeparture, 
            DateTime utcArrival, 
            string carrier, 
            decimal? price = null)
        {
            this.From = Validator.ReturnOrThrowIfNullOrWhiteSpace(from);
            this.To = Validator.ReturnOrThrowIfNullOrWhiteSpace(to);
            this.UtcDeparture = utcDeparture;
            this.UtcArrival = utcArrival;
            this.Carrier = Validator.ReturnOrThrowIfNullOrWhiteSpace(carrier);
            this.Price = price;
        }

        public string From { get; private set; }

        public string To { get; private set; }

        public DateTime UtcArrival { get; private set; }

        public DateTime UtcDeparture { get; private set; }

        public TimeSpan Duration => this.UtcArrival - this.UtcDeparture;

        public string Carrier { get; private set; }

        public decimal? Price { get; private set; }

        public override string ToString()
        {
            return $"{this.From} - {this.To} ({this.UtcDeparture} - {this.UtcArrival}) {this.Price}";
        }
    }
}
