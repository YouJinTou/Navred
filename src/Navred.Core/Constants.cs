using Navred.Core.Itineraries;

namespace Navred.Core
{
    public static class Constants
    {
        public const string StageUpper = "STAGE";

        public const DaysOfWeek AllWeek =
            DaysOfWeek.Friday |
            DaysOfWeek.Monday |
            DaysOfWeek.Saturday |
            DaysOfWeek.Sunday |
            DaysOfWeek.Thursday |
            DaysOfWeek.Tuesday |
            DaysOfWeek.Wednesday;

        public const DaysOfWeek MondayToFriday =
            DaysOfWeek.Monday |
            DaysOfWeek.Tuesday |
            DaysOfWeek.Wednesday |
            DaysOfWeek.Thursday |
            DaysOfWeek.Friday;

        public const DaysOfWeek Weekend = DaysOfWeek.Saturday | DaysOfWeek.Sunday;

        public const double EarthRadiusInKm = 6371;

        public const string UtcTimeZone = "UTC";

        public const string BulgariaTimeZone = "FLE Standard Time";

        public const string UnknownCarrier = "Unknown";
    }
}
