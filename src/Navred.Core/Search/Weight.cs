using System;

namespace Navred.Core.Search
{
    public class Weight : IComparable<Weight>, IEquatable<Weight>
    {
        public TimeSpan Duration { get; set; }

        public decimal? Price { get; set; }

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
            var w = new Weight
            {
                Duration = w1.Duration + w2.Duration,
                Price = (w1.Price ?? 0m) + (w2.Price ?? 0m)
            };

            return w;
        }

        public static Weight operator -(Weight w1, Weight w2)
        {
            var w = new Weight
            {
                Duration = w1.Duration - w2.Duration,
                Price = (w1.Price ?? 0m) - (w2.Price ?? 0m)
            };

            return w;
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
