using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Places;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Models
{
    public class Stop
    {
        public string Name { get; set; }

        public string Region { get; set; }

        public string Municipality { get; set; }

        public LegTime Time { get; set; }

        public string Address { get; set; }

        public string Price { get; set; }

        public Place Place { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is Stop other))
            {
                return false;
            }

            return 
                this.Name.Equals(other.Place) && 
                this.Municipality.Equals(other.Municipality) && 
                this.Region.Equals(other.Region);
        }

        public override int GetHashCode()
        {
            int prime = 83;
            int result = 1;

            unchecked
            {
                result *= prime + this.Name.GetHashCode();
                result *= prime + this.Region?.GetHashCode() ?? prime;
                result *= prime + this.Municipality?.GetHashCode() ?? prime;
            }

            return result;
        }

        public static IEnumerable<Stop> CreateMany(
            IEnumerable<string> names,
            IEnumerable<string> times,
            IEnumerable<string> prices = null,
            IEnumerable<string> addresses = null)
        {
            if (!names.Count().Equals(times.Count()))
            {
                throw new ArgumentException("Stops count mismatch.");
            }

            if (!addresses?.IsNullOrEmpty() ?? false &&
                !addresses.Count().Equals(names.Count()))
            {
                throw new ArgumentException("Addresses count mismatch.");
            }

            if (!prices?.IsNullOrEmpty() ?? false &&
                !prices.Count().Equals(names.Count()))
            {
                throw new ArgumentException("Prices count mismatch.");
            }

            var namesList = names.ToList();
            var timesList = times.ToList();
            var pricesList = prices?.ToList();
            var addressesList = addresses?.ToList();
            var stops = new List<Stop>();

            for (int i = 0; i < namesList.Count; i++)
            {
                stops.Add(new Stop
                {
                    Address = addressesList?[i],
                    Name = namesList[i],
                    Price = pricesList?[i],
                    Time = timesList[i]
                });
            }

            return stops;
        }
    }
}
