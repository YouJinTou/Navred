using System;
using System.Collections.Generic;
using System.Linq;

namespace Navred.Core.Itineraries.DB
{
    public class DBTo
    {
        public string To { get; set; }

        public decimal? Price { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime UtcDeparture { get; set; }

        public DateTime UtcArrival { get; set; }

        public IEnumerable<Leg> Legs { get; set; }

        public string GetUniqueId()
        {
            var carriers = this.Legs.Select(s => s.Carrier).Distinct();
            var id = $"{this.To}_{string.Join('_', carriers)}";

            return id;
        }

        public override string ToString()
        {
            return $"{this.To} {this.UtcDeparture} - {this.UtcArrival}";
        }
    }
}
