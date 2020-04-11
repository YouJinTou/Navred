using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers
{
    public class Boydevi : ICrawler
    {
        private readonly ILegRepository repo;
        private readonly IPlacesManager placesManager;
        private readonly ILogger<Boydevi> logger;
        private readonly ICollection<string> stopTrims;

        public Boydevi(ILegRepository repo, IPlacesManager placesManager, ILogger<Boydevi> logger)
        {
            this.repo = repo;
            this.placesManager = placesManager;
            this.logger = logger;
            this.stopTrims = new HashSet<string> { "АГ", "Централна" };
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
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var scheduleStrings = doc.DocumentNode.SelectNodes(
                "//div[@class='entry-content']/p")[2].InnerText.Split("\n");
            var legs = new List<Leg>();

            foreach (var scheduleString in scheduleStrings)
            {
                try
                {
                    var schedule = new Schedule();
                    var daysOfWeek = this.GetDaysOfWeek(scheduleString);
                    var stopMatches = Regex.Matches(
                        scheduleString, @$"([{BCP.AllLetters} .]+)\s*\((\d+:\d+)\)")
                        .ToList();
                    var legSpread = Defaults.DaysAhead;

                    for (int i = 0; i < stopMatches.Count - 1; i++)
                    {
                        var fromMatch = stopMatches[i];
                        var toMatch = stopMatches[i + 1];
                        var formattedFrom = fromMatch.Groups[1].Value.ChainReplace(this.stopTrims);
                        var formattedTo = toMatch.Groups[1].Value.ChainReplace(this.stopTrims);
                        var from = this.placesManager.GetPlace(BCP.CountryName, formattedFrom);
                        var to = this.placesManager.GetPlace(BCP.CountryName, formattedTo);
                        var departureTimes = daysOfWeek.GetValidUtcTimesAhead(
                            fromMatch.Groups[2].Value, Defaults.DaysAhead).ToList();
                        var arrivalTimes = daysOfWeek.GetValidUtcTimesAhead(
                            toMatch.Groups[2].Value, Defaults.DaysAhead).ToList();
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
                catch (Exception ex)
                {
                    this.logger.LogError(ex, scheduleString);
                }
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

            this.logger.LogWarning($"Could not match days of week for {scheduleString}");

            return daysOfWeek;
        }
    }
}
