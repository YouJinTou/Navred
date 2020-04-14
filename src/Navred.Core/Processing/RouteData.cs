using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Processing
{
    public class RouteData
    {
        public RouteData(
            string country,
            DaysOfWeek dow,
            string carrier,
            Mode mode,
            IEnumerable<LegTime> stopTimes,
            IEnumerable<string> stops,
            IEnumerable<string> addresses = null,
            IEnumerable<string> prices = null,
            string info = null)
        {
            this.Country = Validator.ReturnOrThrowIfNullOrWhiteSpace(country, "Country is empty.");
            this.Mode = mode;
            this.DaysOfWeek = dow;
            this.Carrier = Validator.ReturnOrThrowIfNullOrWhiteSpace(carrier, "Carrier is empty.");
            this.StopTimes = Validator.ReturnOrThrowIfNullOrEmpty(
                stopTimes, "Stop times empty.").ToList();
            this.Stops = Validator.ReturnOrThrowIfNullOrEmpty(stops, "Stops empty.").ToList();

            if (!this.StopTimes.Count.Equals(this.Stops.Count))
            {
                throw new ArgumentException("Stops count mismatch.");
            }

            if (!this.Addresses.IsNullOrEmpty() && !this.Addresses.Count.Equals(this.Stops.Count))
            {
                throw new ArgumentException("Addresses count mismatch.");
            }

            if (!this.Prices.IsNullOrEmpty() && !this.Prices.Count.Equals(this.Stops.Count))
            {
                throw new ArgumentException("Prices count mismatch.");
            }

            this.Addresses = addresses?.ToList();
            this.Prices = prices?.ToList();
            this.Info = info;
        }

        public string Country { get; }

        public Mode Mode { get; set; }

        public DaysOfWeek DaysOfWeek { get; }

        public string Carrier { get; }

        public string Info { get; }

        public IList<LegTime> StopTimes { get; }

        public IList<string> Stops { get; }

        public IList<string> Addresses { get; }

        public IList<string> Prices { get; }

        public RouteData Copy()
        {
            return new RouteData(
                this.Country,
                this.DaysOfWeek,
                this.Carrier,
                this.Mode,
                this.StopTimes,
                this.Stops,
                this.Addresses,
                this.Prices,
                this.Info);
        }
    }
}
