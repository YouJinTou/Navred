using System;

namespace Navred.Core.Itineraries.DB
{
    public class DBTo
    {
        public string To { get; set; }

        public string Carrier { get; set; }

        public decimal? Price { get; set; }

        public DaysOfWeek? OnDays { get; set; }

        public TimeSpan Duration { get; set; }

        public DateTime Departure { get; set; }

        public DateTime Arrival { get; set; }

        public string GetUniqueId()
        {
            return $"{this.To}_{this.Carrier}";
        }

        public override string ToString()
        {
            return $"{this.To} {this.Departure} - {this.Arrival}";
        }
    }
}
