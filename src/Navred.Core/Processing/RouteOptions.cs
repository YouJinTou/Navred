using System;

namespace Navred.Core.Processing
{
    [Flags]
    public enum RouteOptions
    {
        None = 0,
        RemoveDuplicates = 1 << 1,
        EstimateDuplicates = 1 << 2,
        AdjustInvalidArrivals = 1 << 3,
        EstimateDepartureOrArrival = 1 << 4
    }
}
