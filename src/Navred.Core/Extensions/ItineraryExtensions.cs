using System.Linq;

namespace Navred.Core.Extensions
{
    public static class ItineraryExtensions
    {
        public static string FormatId(this string id)
        {
            var tokens = id.Split('|').Skip(1).ToList();

            if (tokens.IsEmpty())
            {
                return id;
            }

            var name = $"{tokens[0]} ({string.Join(", ", tokens.Skip(1))})";

            return name;
        }
    }
}
