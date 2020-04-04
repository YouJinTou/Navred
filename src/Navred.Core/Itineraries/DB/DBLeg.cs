using Navred.Core.Extensions;
using System.Collections.Generic;

namespace Navred.Core.Itineraries.DB
{
    public class DBLeg
    {
        public string From { get; set; }

        public long UtcTimestamp { get; set; }

        public IList<Leg> Tos { get; set; }

        public override string ToString()
        {
            return $"{this.From.FormatId()} {this.UtcTimestamp.ToUtcDateTime()}";
        }
    }
}
