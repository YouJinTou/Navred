using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using Navred.Core.Places;
using System;

namespace Navred.Core.Itineraries
{
    public class Leg : ICopyable<Leg>
    {
        public Leg()
        {
        }

        public Leg(
            Place from,
            Place to,
            DateTime utcDeparture,
            DateTime utcArrival,
            string carrier,
            Mode mode,
            string info,
            decimal? price = null,
            string fromSpecific = null,
            string toSpecific = null,
            bool arrivalEstimated = false,
            bool departureEstimated = false,
            bool priceEstimated = false)
        {
            if (utcDeparture >= utcArrival)
            {
                throw new ArgumentException("Departure after arrival.");
            }

            this.From = from.ReturnOrThrowIfNull("Source empty.");
            this.To = to.ReturnOrThrowIfNull("Destination empty.");
            this.FromId = this.From.GetId();
            this.ToId = this.To.GetId();
            this.UtcDeparture = utcDeparture;
            this.UtcArrival = utcArrival;
            this.Carrier = string.IsNullOrWhiteSpace(carrier) ? Constants.UnknownCarrier : carrier;
            this.Mode = mode;
            this.Info = info;
            this.Price = price;
            this.FromSpecific = fromSpecific?.Trim();
            this.ToSpecific = toSpecific?.Trim();
            this.DepartureEstimated = departureEstimated;
            this.ArrivalEstimated = arrivalEstimated;
            this.PriceEstimated = priceEstimated;
        }

        public Place From { get; private set; }

        public string FromId { get; private set; }

        public Place To { get; private set; }

        public string ToId { get; private set; }

        public DateTime UtcArrival { get; private set; }

        public DateTime UtcDeparture { get; private set; }

        public TimeSpan Duration
        {
            get => this.UtcArrival - this.UtcDeparture;
            private set { }
        }

        public string Carrier { get; private set; }

        public Mode Mode { get; private set; }

        public string Info { get; private set; }

        public decimal? Price { get; set; }

        public string FromSpecific { get; private set; }

        public string ToSpecific { get; private set; }

        public bool DepartureEstimated { get; private set; }

        public bool ArrivalEstimated { get; private set; }

        public bool PriceEstimated { get; set; }

        public string GetUniqueId()
        {
            var id = $"{this.To}_{this.Carrier}_{this.UtcArrival}_{this.UtcDeparture}";

            return id;
        }

        public bool IsZeroLength()
        {
            return this.From.Equals(this.To);
        }

        public bool TryMergeWith(Leg other, out Leg merged)
        {
            var isValidMerge =
                !other.IsNull() &&
                (this.UtcDeparture < other.UtcArrival) &&
                this.To.Equals(other.From) &&
                this.Carrier.Equals(other.Carrier) &&
                this.Mode.Equals(other.Mode) &&
                (this.Price.HasValue && other.Price.HasValue);
            merged = null;

            if (!isValidMerge)
            {
                return false;
            }

            var info = this.Info?.Equals(other.Info) ?? true ?
                this.Info : $"{this.Info} | {other.Info}";
            merged = new Leg(
                this.From,
                other.To,
                this.UtcDeparture,
                other.UtcArrival,
                this.Carrier,
                this.Mode,
                info,
                this.Price + other.Price,
                this.FromSpecific,
                other.ToSpecific,
                this.DepartureEstimated || other.DepartureEstimated,
                this.ArrivalEstimated || other.ArrivalEstimated,
                this.PriceEstimated || other.PriceEstimated);

            return true;
        }

        public override string ToString()
        {
            return
                $"{this.From ?? this.FromId.FormatId()} {this.UtcDeparture} - " +
                $"{this.To ?? this.ToId.FormatId()} {this.UtcArrival} {this.Price}";
        }

        public Leg Copy()
        {
            return new Leg(
                this.From.Copy(), 
                this.To.Copy(), 
                this.UtcDeparture, 
                this.UtcArrival, 
                this.Carrier, 
                this.Mode, 
                this.Info, 
                this.Price, 
                this.FromSpecific, 
                this.ToSpecific, 
                this.DepartureEstimated,
                this.ArrivalEstimated,
                this.PriceEstimated);
        }

        public Leg CopyReverse()
        {
            return new Leg(
                this.To.Copy(),
                this.From.Copy(),
                this.UtcDeparture,
                this.UtcArrival,
                this.Carrier,
                this.Mode,
                this.Info,
                this.Price,
                this.ToSpecific,
                this.FromSpecific,
                this.DepartureEstimated,
                this.ArrivalEstimated,
                this.PriceEstimated);
        }
    }
}
