using System;

namespace Navred.Core.Search
{
    public class Weight
    {
        public TimeSpan Duration { get; set; }

        public decimal? Price { get; set; }

        public static Weight CreateMax()
        {
            return new Weight
            {
                Duration = TimeSpan.MaxValue,
                Price = decimal.MaxValue
            };
        }

        public override string ToString()
        {
            return $"{this.Duration}" + (this.Price.HasValue ? $" | {this.Price}" : string.Empty);
        }
    }
}
