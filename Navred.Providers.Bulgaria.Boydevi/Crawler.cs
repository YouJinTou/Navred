using HtmlAgilityPack;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.Boydevi
{
    public class Crawler : ICrawler
    {
        private readonly IItineraryRepository repo;

        public Crawler(IItineraryRepository repo)
        {
            this.repo = repo;
        }

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

            await repo.UpdateItinerariesAsync(itineraries);

            return itineraries;
        }

        private async Task<IEnumerable<Itinerary>> GetItinerariesAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var scheduleStrings = doc.DocumentNode.SelectNodes(
                "//div[@class='entry-content']/p")[2].InnerText.Split("\n");
            var daysAhead = 30;
            var schedule = new Schedule();

            foreach (var scheduleString in scheduleStrings)
            {
                var currentItineraries = new List<Itinerary>();
                var daysOfWeek = this.GetDaysOfWeek(scheduleString);
                var stopMatches = Regex.Matches(
                    scheduleString, @$"([{BulgarianCultureProvider.Letters} .]+)\s*\((\d+:\d+)\)")
                    .ToList();

                for (int i = 0; i < stopMatches.Count; i++)
                {
                    var match = stopMatches[i];
                    var name = match.Groups[1].Value.Replace("АГ", string.Empty).Trim();
                    var arrivalTime = match.Groups[2].Value;
                    var arrivalTimes = daysOfWeek.GetValidUtcTimesAhead(arrivalTime, daysAhead)
                        .ToList();

                    if (currentItineraries.IsEmpty())
                    {
                        currentItineraries = Enumerable.Range(0, arrivalTimes.Count)
                            .Select(i => new Itinerary("Бойдеви")).ToList();
                    }

                    for (int t = 0; t < arrivalTimes.Count(); t++)
                    {
                        currentItineraries[t].AddStop(new Stop(name, arrivalTimes[t]));
                    }
                }

                schedule.AddItineraries(currentItineraries);
            }

            var itineraries = schedule.GetWithChildren();

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
