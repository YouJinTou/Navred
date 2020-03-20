using System;

namespace Navred.Core.Search
{
    public class Weight
    {
        public TimeSpan Duration { get; set; }

        public decimal? Price { get; set; }

        public DateTime UtcArrival { get; set; }

        public string Carrier { get; set; }

        public static Weight CreateMax()
        {
            return new Weight
            {
                Duration = TimeSpan.MaxValue,
                Price = decimal.MaxValue
            };
        }

        public static bool operator <(Weight w1, Weight w2)
        {
            return w1.Duration < w2.Duration;
        }

        public static bool operator >(Weight w1, Weight w2)
        {
            return w1.Duration > w2.Duration;
        }

        public static Weight operator +(Weight w1, Weight w2)
        {
            return new Weight
            {
                Duration = w1.Duration + w2.Duration,
                Price = w1.Price + w2.Price,
                UtcArrival = w2.UtcArrival
            };
        }

        public override string ToString()
        {
            return $"{this.Duration}" + (this.Price.HasValue ? $" | {this.Price}" : string.Empty);
        }
    }
}
