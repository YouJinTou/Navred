using System;

namespace Navred.Core.Search
{
    public class Weight : IComparable<Weight>, IEquatable<Weight>
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
                Price = w1.Price + w2.Price
            };
        }

        public override string ToString()
        {
            return $"{this.Duration}" + (this.Price.HasValue ? $" | {this.Price}" : string.Empty);
        }

        public int CompareTo(Weight other)
        {
            if (other == null)
            {
                return -1;
            }

            return this.Duration.CompareTo(other.Duration);
        }

        public bool Equals(Weight other)
        {
            if (other == null)
            {
                return false;
            }

            return this.Duration.Equals(other.Duration) && this.Price.Equals(other.Price);
        }
    }
}
