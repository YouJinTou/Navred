using Navred.Core.Itineraries;
using System;
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

        IEnumerable<DateTime> GetHolidays();

        decimal? ParsePrice(string priceString);

        Encoding GetEncoding();
    }
}
