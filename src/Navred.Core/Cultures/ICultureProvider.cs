using Navred.Core.Itineraries;
using System.Collections.Generic;
using System.Text;

namespace Navred.Core.Cultures
{
    public interface ICultureProvider
    {
        string Name { get; }

        string Letters { get; }

        string Latinize(string s);

        DaysOfWeek ToDaysOfWeek(string dayString);

        DaysOfWeek ToDaysOfWeek(IEnumerable<string> dayStrings);

        decimal? ParsePrice(string priceString);

        Encoding GetEncoding();
    }
}
