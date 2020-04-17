using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Models;
using Navred.Core.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers.Companies
{
    public class Boydevi : ICrawler
    {
        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly ILogger<Boydevi> logger;
        private readonly ICollection<string> stopTrims;

        public Boydevi(IRouteParser routeParser, ILegRepository repo, ILogger<Boydevi> logger)
        {
            this.routeParser = routeParser;
            this.repo = repo;
            this.logger = logger;
            this.stopTrims = new HashSet<string> { "АГ", "АГ Юг", "Централна" };
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
            var all = new List<Leg>();

            foreach (var scheduleString in scheduleStrings)
            {
                var dow = this.GetDaysOfWeek(scheduleString);
                var matches = Regex.Matches(
                    scheduleString, @$"([{BCP.AllLetters} .]+)\s*\((\d+:\d+)\)");
                var names = matches
                    .Select(m => m.Groups[1].Value.ChainReplace(this.stopTrims))
                    .ToList();
                var addresses = matches.Select(m => m.Groups[1].Value);
                var times = matches.Select(m => m.Groups[2].Value);
                var stops = Stop.CreateMany(names, times, addresses: addresses);
                var route = new Route(BCP.CountryName, dow, "Бойдеви", Mode.Bus, stops, url);
                var legs = await this.routeParser.ParseRouteAsync(route);

                all.AddRange(legs);
            }

            return all;
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
                daysOfWeek = isFound ? 
                    daysOfWeek | DaysOfWeek.HolidayInclusive : DaysOfWeek.HolidayInclusive;
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
