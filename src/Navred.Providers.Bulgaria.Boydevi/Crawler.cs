using HtmlAgilityPack;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.Boydevi
{
    public class Crawler : ICrawler
    {
        private readonly ILegRepository repo;
        private readonly IPlacesManager placesManager;

        public Crawler(ILegRepository repo, IPlacesManager placesManager)
        {
            this.repo = repo;
            this.placesManager = placesManager;
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var legs = new List<Leg>();
                var svilengradSofia = await this.GetLegsAsync(
                    "http://boydevi-bg.com/%d0%b7%d0%b0-%d1%81%d0%be%d1%84%d0%b8%d1%8f/");
                var svilengradHaskovo = await this.GetLegsAsync(
                "http://boydevi-bg.com/%d0%b7%d0%b0-%d1%85%d0%b0%d1%81%d0%ba%d0%be%d0%b2%d0%be/");
                var toSvilengrad = await this.GetLegsAsync(
                    "http://boydevi-bg.com/%d0%b7%d0%b0-%d1%81%d0%b2%d0%b8%d0%bb%d0%b5%d0%bd%d0%b3%d1%80%d0%b0%d0%b4/");

                legs.AddRange(svilengradSofia);

                legs.AddRange(svilengradHaskovo);

                legs.AddRange(toSvilengrad);

                await repo.UpdateLegsAsync(legs);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var scheduleStrings = doc.DocumentNode.SelectNodes(
                "//div[@class='entry-content']/p")[2].InnerText.Split("\n");
            var daysAhead = 30;
            var legs = new List<Leg>();

            foreach (var scheduleString in scheduleStrings)
            {
                var schedule = new Schedule();
                var daysOfWeek = this.GetDaysOfWeek(scheduleString);
                var stopMatches = Regex.Matches(
                    scheduleString, @$"([{BulgarianCultureProvider.AllLetters} .]+)\s*\((\d+:\d+)\)")
                    .ToList();
                var legSpread = daysAhead;

                for (int i = 0; i < stopMatches.Count - 1; i++)
                {
                    var fromMatch = stopMatches[i];
                    var toMatch = stopMatches[i + 1];
                    var from = this.placesManager.GetPlace(
                        BulgarianCultureProvider.CountryName, fromMatch.Groups[1].Value);
                    var to = this.placesManager.GetPlace(
                        BulgarianCultureProvider.CountryName, toMatch.Groups[1].Value);
                    var departureTimes = daysOfWeek.GetValidUtcTimesAhead(
                        fromMatch.Groups[2].Value, daysAhead).ToList();
                    var arrivalTimes = daysOfWeek.GetValidUtcTimesAhead(
                        toMatch.Groups[2].Value, daysAhead).ToList();
                    legSpread = arrivalTimes.Count;

                    for (int t = 0; t < arrivalTimes.Count; t++)
                    {
                        schedule.AddLeg(new Leg(
                            from, 
                            to, 
                            departureTimes[t], 
                            arrivalTimes[t], 
                            "Бойдеви", 
                            Mode.Bus,
                            url,
                            fromSpecific: fromMatch.Groups[1].Value,
                            toSpecific: toMatch.Groups[1].Value));
                    }
                }

                legs.AddRange(schedule.GetWithChildren(legSpread));
            }

            return legs;
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
