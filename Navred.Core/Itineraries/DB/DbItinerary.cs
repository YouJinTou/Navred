﻿using System.Collections.Generic;

namespace Navred.Core.Itineraries.DB
{
    public class DBItinerary
    {
        public string From { get; set; }

        public long UtcTimestamp { get; set; }

        public IEnumerable<DBTo> Tos { get; set; }
    }
}
