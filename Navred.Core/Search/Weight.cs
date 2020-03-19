using System;

namespace Navred.Core.Search
{
    internal class Weight
    {
        public TimeSpan Duration { get; set; }

        public decimal? Price { get; set; }

        public override string ToString()
        {
            return $"{this.Duration}" + (this.Price.HasValue ? $" | {this.Price}" : string.Empty);
        }
    }
}
