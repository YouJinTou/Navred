using Navred.Core.Extensions;
using System;

namespace Navred.Core.Itineraries.DB
{
    public class TimeWindow
    {
        public TimeWindow(DateTime from, DateTime to)
        {
            if (to <= from)
            {
                throw new ArgumentException("Negative time window.");
            }

            this.LocalFrom = from;
            this.LocalTo = to;
        }

        public DateTime LocalFrom { get; }

        public DateTime LocalTo { get; }

        public long FromUtcTimestamp => this.LocalFrom.ToUtcTimestamp();

        public long ToUtcTimestmap => this.LocalTo.ToUtcTimestamp();

        public override string ToString()
        {
            return $"{this.LocalFrom} - {this.LocalTo}";
        }
    }
}
