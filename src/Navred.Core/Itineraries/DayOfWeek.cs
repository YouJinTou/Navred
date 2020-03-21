using System;

namespace Navred.Core.Itineraries
{
    [Flags]
    public enum DaysOfWeek
    {
        Monday = 1 << 1,
        Tuesday = 1 << 2,
        Wednesday = 1 << 3,
        Thursday = 1 << 4,
        Friday = 1 << 5,
        Saturday = 1 << 6,
        Sunday = 1 << 7,
        Holiday = 1 << 8
    }
}
