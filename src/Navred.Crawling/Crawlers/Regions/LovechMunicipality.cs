using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
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
using System.Web;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers.Regions
{
    public class LovechMunicipality : ICrawler
    {
        private const string Url = "https://www.lovech.bg/bg/transport/razpisanie-na-mezhduselishtnite-avtobusni-linii-v-obshtina-lovech";

        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly ILogger<LovechMunicipality> logger;
        private readonly IDictionary<string, string> replacements;

        public LovechMunicipality(
            IRouteParser routeParser,
            ILegRepository repo,
            ILogger<LovechMunicipality> logger)
        {
            this.routeParser = routeParser;
            this.repo = repo;
            this.logger = logger;
            this.replacements = new Dictionary<string, string>
            {
                { "Автогара", BCP.City.Lovech },
                { "с.", string.Empty },
                { "ЖПС-", "ЖПС" },
                { "ЖПС", "ЖПС-" },
                { "Горно Павликени", "Горно Павликене" },
            };
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var legs = await this.GetLegsAsync(Url);

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
            var rows = doc.DocumentNode
                .SelectSingleNode("//table").SelectNodes(".//tr").Skip(4).ToList();
            var all = new List<Leg>();

            foreach (var row in rows)
            {
                var data = row.SelectNodes(".//td").ToList();
                var place1 = data[1].InnerText.Split('-').First().Trim().ToLower();
                var place2 = data[1].InnerText.Split('-').Last().Trim().ToLower();
                var namesBase = data[3].InnerText
                    .ChainReplace(this.replacements, false).ToLower().Trim().Split('-');
                var departureNames = namesBase
                    .TakeWhileInclusive(s => s.ToLower().Equals(place2))
                    .ToDictionary(kvp => kvp, kvp => BCP.Region.LOV);
                var arrivalNames = namesBase
                    .Reverse()
                    .SkipUntilLast(place2)
                    .ToDictionary(kvp => kvp, kvp => BCP.Region.LOV);
                var weekDayDepartures = this.GetStopTimes(data[4], departureNames.Count);
                var weekDayArrivals = this.GetStopTimes(data[5], arrivalNames.Count);
                var weekendDepartures = this.GetStopTimes(data[6], departureNames.Count);
                var weekendArrivals = this.GetStopTimes(data[7], arrivalNames.Count);
                var legs = new List<Leg>();
                var weekDayDepartureLegs = await this.GetLegsAsync(
                    weekDayDepartures, departureNames, Constants.MondayToFriday);
                var weekDayArrivalLegs = await this.GetLegsAsync(
                    weekDayArrivals, arrivalNames, Constants.MondayToFriday);
                var weekendDepartureLegs = await this.GetLegsAsync(
                    weekendDepartures, departureNames, Constants.Weekend | DaysOfWeek.HolidayInclusive);
                var weekendArrivalLegs = await this.GetLegsAsync(
                    weekendArrivals, arrivalNames, Constants.Weekend | DaysOfWeek.HolidayInclusive);

                legs.AddRange(weekDayDepartureLegs);
                legs.AddRange(weekDayArrivalLegs);
                legs.AddRange(weekendDepartureLegs);
                legs.AddRange(weekendArrivalLegs);
                all.AddRange(legs);
            }

            return all;
        }

        private IList<IEnumerable<string>> GetStopTimes(HtmlNode node, int count)
        {
            var times = node
                .SelectNodes(".//p")
                .Where(n => !string.IsNullOrWhiteSpace(HttpUtility.HtmlDecode(n.InnerText)))
                .Select(n => new[]
                {
                    HttpUtility.HtmlDecode(n.InnerText),
                    null
                }.InsertBetween(null, count))
                .ToList();

            return times;
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync(
            IList<IEnumerable<string>> timesList,
            IDictionary<string, string> names,
            DaysOfWeek dow)
        {
            var all = new List<Leg>();
            var schedule = string.Join(" ", timesList.SelectMany(l => l.Select(s => s)));
            var dows = Regex.Matches(schedule, @"(\d{1,2}[.,:]\d{1,2})([\D]+)");

            for (int t = 0, m = 0; t < timesList.Count; t++, m++)
            {
                try
                {
                    var match = dows[m];
                    var stops = Stop.CreateMany(names.Keys, timesList[t], regions: names.Values);
                    var route = new Route(
                        BulgarianCultureProvider.CountryName,
                        this.GetDow(match.Groups[2].Value, dow),
                        "Община Ловеч",
                        Mode.Bus,
                        stops,
                        Url);
                    var legs = await this.routeParser.ParseRouteAsync(route);
                    t = string.IsNullOrWhiteSpace(match.Groups[2].Value) ? t : t + 1;

                    all.AddRange(legs);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(
                        ex, $"{string.Join(" ", names)} | {string.Join(" ", timesList[t])}");
                }
            }

            return all;
        }

        private DaysOfWeek GetDow(string s, DaysOfWeek current)
        {
            var result = Regex.Replace(s.Trim(), $@"[^{BCP.AllLetters}]", string.Empty);

            return result switch
            {
                "с" => DaysOfWeek.Saturday,
                "н" => DaysOfWeek.Sunday,
                "ср" => DaysOfWeek.Wednesday,
                "всряда" => DaysOfWeek.Wednesday,
                "всрядаипетък" => DaysOfWeek.Wednesday | DaysOfWeek.Friday,
                "внед" => DaysOfWeek.Sunday,
                _ => current
            };
        }
    }
}
