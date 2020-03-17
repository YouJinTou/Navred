using Navred.Core.Models;

namespace Navred.Core
{
    public static class Constants
    {
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
    }
}
