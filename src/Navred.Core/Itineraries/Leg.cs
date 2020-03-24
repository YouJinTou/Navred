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
            Mode mode,
            decimal? price = null,
            string fromSpecific = null,
            string toSpecific = null,
            bool arrivalEstimated = false)
        {
            this.From = Validator.ReturnOrThrowIfNullOrWhiteSpace(from);
            this.To = Validator.ReturnOrThrowIfNullOrWhiteSpace(to);
            this.UtcDeparture = utcDeparture;
            this.UtcArrival = utcArrival;
            this.Carrier = Validator.ReturnOrThrowIfNullOrWhiteSpace(carrier);
            this.Mode = mode;
            this.Price = price;
            this.FromSpecific = fromSpecific?.Trim();
            this.ToSpecific = toSpecific?.Trim();
            this.ArrivalEstimated = arrivalEstimated;
        }

        public string From { get; private set; }

        public string To { get; private set; }

        public DateTime UtcArrival { get; private set; }

        public DateTime UtcDeparture { get; private set; }

        public TimeSpan Duration
        {
            get => this.UtcArrival - this.UtcDeparture;
            private set { }
        }

        public string Carrier { get; private set; }

        public Mode Mode { get; private set; }

        public decimal? Price { get; private set; }

        public string FromSpecific { get; private set; }

        public string ToSpecific { get; private set; }

        public bool ArrivalEstimated { get; private set; }

        public string GetUniqueId()
        {
            var id = $"{this.To}_{this.Carrier}";

            return id;
        }

        public override string ToString()
        {
            return 
                $"{this.From} {this.UtcDeparture} - {this.To} {this.UtcArrival} {this.Price}";
        }
    }
}
