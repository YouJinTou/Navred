using Navred.Core.Extensions;
using Navred.Core.Models;
using Navred.Core.Tools;
using System;

namespace Navred.Core.Itineraries
{
    public class TimeWindow
    {
        public TimeWindow(DateTimeTz from, DateTimeTz to)
        {
            Validator.ThrowIfAnyNullOrWhiteSpace(from, to);

            if (to <= from)
            {
                throw new ArgumentException("Negative time window.");
            }

            this.From = from;
            this.To = to;
        }

        public DateTimeTz From { get; }

        public DateTimeTz To { get; }

        public long FromUtcTimestamp => this.From.DateTime.ToUtcTimestamp(this.From.TimeZone);

        public long ToUtcTimestmap => this.To.DateTime.ToUtcTimestamp(this.To.TimeZone);

        public override string ToString()
        {
            return $"{this.From.DateTime} - {this.To.DateTime}";
        }
    }
}
