using Navred.Core.Places;
using System;

namespace Navred.Crawling.Models
{
    public class StopInfo
    {
        public Place Stop { get; set; }

        public TimeSpan Time { get; set; }

        public decimal? Price { get; set; }

        public override string ToString()
        {
            return $"{this.Stop} {this.Time} {this.Price}";
        }
    }
}
