using System;

namespace Navred.Core.Search
{
    public class Weight : IComparable<Weight>
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

        public int CompareTo(Weight other)
        {
            if (other == null)
            {
                return -1;
            }

            return this.Duration.CompareTo(other.Duration);
        }

        public override string ToString()
        {
            return $"{this.Duration}" + (this.Price.HasValue ? $" | {this.Price}" : string.Empty);
        }
    }
}
