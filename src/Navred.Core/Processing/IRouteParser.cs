﻿using Navred.Core.Itineraries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Processing
{
    public interface IRouteParser
    {
        Route Parsed { get; }

        Task<IEnumerable<Leg>> ParseRouteAsync(Route route);
    }
}