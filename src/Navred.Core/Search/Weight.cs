using Navred.Core.Abstractions;
using System;

namespace Navred.Core.Search
{
    public class Weight : IComparable<Weight>, IEquatable<Weight>, ICopyable<Weight>
    {
        public TimeSpan Duration { get; set; }

        public decimal? Price { get; set; }

        public static Weight Max()
        {
            return new Weight
            {
                Duration = TimeSpan.MaxValue,
                Price = decimal.MaxValue
            };
        }

        public static Weight Zero()
        {
            return new Weight
            {
                Duration = new TimeSpan(),
                Price = null
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
            var w = new Weight
            {
                Duration = w1.Duration + w2.Duration,
                Price = !(w1.Price.HasValue && w2.Price.HasValue) ? 
                    (decimal?)null : (w1.Price ?? 0m) + (w2.Price ?? 0m)
            };

            return w;
        }

        public static Weight operator -(Weight w1, Weight w2)
        {
            var w = new Weight
            {
                Duration = w1.Duration - w2.Duration,
                Price = !(w1.Price.HasValue && w2.Price.HasValue) ?
                    (decimal?)null : (w1.Price ?? 0m) - (w2.Price ?? 0m)
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

        public override int GetHashCode()
        {
            int prime = 83;
            int result = 1;

            unchecked
            {
                result *= prime + this.Duration.GetHashCode();
                result *= prime + (this.Price?.GetHashCode() ?? prime);
            }

            return result;
        }

        public Weight Copy()
        {
            return new Weight
            {
                Duration = this.Duration,
                Price = this.Price
            };
        }
    }
}
