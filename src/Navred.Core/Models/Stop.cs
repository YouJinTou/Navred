using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Places;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Models
{
    public class Stop : ICopyable<Stop>, IEquatable<Stop>
    {
        public Stop(
            string name,
            string region,
            string municipality,
            LegTime time,
            string address,
            StopPrice price,
            Place place = null)
        {
            this.Name = name;
            this.Region = region;
            this.Municipality = municipality;
            this.Time = time;
            this.Address = address;
            this.Price = price;
            this.Place = place;
        }

        public string Name { get; set; }

        public string Region { get; set; }

        public string Municipality { get; set; }

        public LegTime Time { get; set; }

        public string Address { get; set; }

        public StopPrice Price { get; set; }

        public Place Place { get; set; }

        public string CompositeName => $"{this.Name}|{this.Region}|{this.Municipality}";

        public override string ToString()
        {
            return $"{this.Name} {this.Time} {this.Price}";
        }

        public bool Equals(Stop other)
        {
            if (other.IsNull())
            {
                return false;
            }

            if (!this.Name?.Equals(other.Name) ?? false)
            {
                return false;
            }

            if (!this.Region?.Equals(other.Region) ?? false)
            {
                return false;
            }

            if (!this.Municipality?.Equals(other.Municipality) ?? false)
            {
                return false;
            }

            if (!this.Time?.Equals(other.Time) ?? false)
            {
                return false;
            }

            return true;
        }

        public Stop Copy()
        {
            return new Stop(
                this.Name,
                this.Region,
                this.Municipality,
                this.Time.Copy(),
                this.Address,
                this.Price,
                this.Place?.Copy());
        }

        public static IEnumerable<Stop> CreateBanned(params string[] names)
        {
            foreach (var name in names)
            {
                yield return new Stop(name, null, null, LegTime.Estimable, null, null);
            }
        }

        public static IEnumerable<Stop> CreateMany(
            IEnumerable<string> names,
            IEnumerable<string> times,
            IEnumerable<string> prices = null,
            IEnumerable<string> addresses = null,
            IEnumerable<string> timesToMarkAsEstimable = null,
            bool ignoreEmptyTimes = false,
            IEnumerable<string> regions = null)
        {
            if (!names.Count().Equals(times.Count()))
            {
                throw new ArgumentException("Times count mismatch.");
            }

            if (!addresses.IsNullOrEmpty() && !addresses.Count().Equals(names.Count()))
            {
                throw new ArgumentException("Addresses count mismatch.");
            }

            if (!prices.IsNullOrEmpty() && !prices.Count().Equals(names.Count()))
            {
                throw new ArgumentException("Prices count mismatch.");
            }

            if (!regions.IsNullOrEmpty() && !regions.Count().Equals(names.Count()))
            {
                throw new ArgumentException("Regions count mismatch.");
            }

            var namesList = names.ToList();
            var timesList = times.ToList();
            var pricesList = prices?.ToList();
            var addressesList = addresses?.ToList();
            var regionsList = regions?.ToList();
            var stops = new List<Stop>();

            for (int i = 0; i < namesList.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(timesList[i]) && ignoreEmptyTimes)
                {
                    continue;
                }

                var legTime = string.IsNullOrWhiteSpace(timesList[i]) ? LegTime.Estimable :
                    timesToMarkAsEstimable?.Contains(timesList[i]) ?? false ?
                    LegTime.Estimable : timesList[i];

                stops.Add(new Stop(
                    namesList[i].Trim(),
                    regionsList?[i]?.Trim(),
                    null,
                    legTime,
                    addressesList?[i]?.Trim(),
                    new StopPrice(pricesList?[i]?.Trim())));
            }

            return stops;
        }
    }
}
