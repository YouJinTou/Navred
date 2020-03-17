using HtmlAgilityPack;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Models;
using Navred.Core.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.Boydevi
{
    public class Crawler : ICrawler
    {
        public async Task<IEnumerable<Itinerary>> GetItinerariesAsync()
        {
            var itineraries = new List<Itinerary>();
            var svilengradSofia = await this.GetItinerariesAsync(
                "http://boydevi-bg.com/%d0%b7%d0%b0-%d1%81%d0%be%d1%84%d0%b8%d1%8f/");
            var svilengradHaskovo = await this.GetItinerariesAsync(
                "http://boydevi-bg.com/%d0%b7%d0%b0-%d1%85%d0%b0%d1%81%d0%ba%d0%be%d0%b2%d0%be/");
            var toSvilengrad = await this.GetItinerariesAsync(
                "http://boydevi-bg.com/%d0%b7%d0%b0-%d1%81%d0%b2%d0%b8%d0%bb%d0%b5%d0%bd%d0%b3%d1%80%d0%b0%d0%b4/");

            itineraries.AddRange(svilengradSofia);

            itineraries.AddRange(svilengradHaskovo);

            itineraries.AddRange(toSvilengrad);

            return itineraries;
        }

        private async Task<IEnumerable<Itinerary>> GetItinerariesAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var scheduleStrings = doc.DocumentNode.SelectNodes(
                "//div[@class='entry-content']/p")[2].InnerText.Split("\n");
            var itineraries = new List<Itinerary>();

            foreach (var scheduleString in scheduleStrings)
            {
                var stops = new List<Stop>();
                var stopMatches = Regex.Matches(
                    scheduleString, @$"([{Bgr.Letters} .]+)\s*\((\d+:\d+)\)")
                    .ToList();

                for (int i = 0; i < stopMatches.Count; i++)
                {
                    var match = stopMatches[i];
                    var name = match.Groups[1].Value.Replace("АГ", string.Empty).Trim();
                    var arrivalTime = match.Groups[2].Value;

                    stops.Add(new Stop(name, arrivalTime));
                }

                var daysOfWeek = this.GetDaysOfWeek(scheduleString);
                var currentItinerary = new Itinerary(stops, "Бойдеви", onDays: daysOfWeek);

                itineraries.AddRange(currentItinerary.ChildrenAndSelf);
            }

            return itineraries;
        }

        private DaysOfWeek GetDaysOfWeek(string scheduleString)
        {
            if (scheduleString.Contains("ежедневен") || scheduleString.Contains("ежедневно"))
            {
                return Constants.AllWeek;
            }

            var isFound = false;
            var daysOfWeek = Constants.AllWeek;

            if (scheduleString.Contains("от понеделник до петък"))
            {
                isFound = true;
                daysOfWeek = Constants.MondayToFriday;
            }

            if (scheduleString.Contains("в неделя"))
            {
                daysOfWeek = isFound ? daysOfWeek | DaysOfWeek.Sunday : DaysOfWeek.Sunday;
                isFound = true;
            }

            if (scheduleString.Contains("празничн"))
            {
                daysOfWeek = isFound ? daysOfWeek | DaysOfWeek.Holiday : DaysOfWeek.Holiday;
                isFound = true;
            }

            if (isFound)
            {
                return daysOfWeek;
            }

            // LOG

            return daysOfWeek;
        }
    }
}
