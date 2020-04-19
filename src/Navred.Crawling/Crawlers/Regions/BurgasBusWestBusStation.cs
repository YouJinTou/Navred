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
using System.Web;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers.Regions
{
    public class BurgasBusWestBusStation : ICrawler
    {
        private const string Url = "http://burgasbus.info/burgasbus/?page_id=69";

        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly ILogger<BurgasBusWestBusStation> logger;
        private readonly ICollection<string> banned;
        private readonly IDictionary<string, string> replacements;
        private readonly IDictionary<string, string> regions;

        public BurgasBusWestBusStation(
            IRouteParser routeParser, ILegRepository repo, ILogger<BurgasBusWestBusStation> logger)
        {
            this.routeParser = routeParser;
            this.repo = repo;
            this.logger = logger;
            this.banned = new HashSet<string>
            {
                "ВАЖНО",
                "часове на връщане от Средец"
            };
            this.replacements = new Dictionary<string, string>
            {
                { "линия", string.Empty },
                { "В. Търново", "Велико Търново" },
                { "Ст. Караджово", "Стефан Караджово" },
                { "Желязово/Орлинци", "Желязово" },
            };
            this.regions = new Dictionary<string, string>
            {
                { "горица", BCP.Region.BGS }
            };
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var legs = await this.GetDeparturesAsync(Url);
                var all = new List<Leg>(legs);

                await repo.UpdateLegsAsync(all);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task<IEnumerable<Leg>> GetDeparturesAsync(string url)
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var ps = doc.DocumentNode.SelectNodes("//p")
                .Select(p => HttpUtility.HtmlDecode(p.InnerText.Trim()))
                .SkipWhile(s => !s.ToLower().Contains("сектор"))
                .ToList();
            var groups = ps.SplitBy(s => s.ToLower().Contains("линия"));
            var allRoutes = new List<Route>();

            foreach (var g in groups)
            {
                try
                {
                    var data = g.Skip(1).Where(s => !s.Contains("сектор"));

                    foreach (var d in data)
                    {
                        var tokens = d.Split("\n").ToList();
                        var carrierMatch = Regex.Match(d, @$"([{BCP.AllLetters}\-\s]+)");
                        var dowTimeMatches =
                            Regex.Matches(d, @"(\d{1,2}:\d{1,2})\s?(?:(?:\/|\()(.*?)(?:\/|\)))?");
                        var dowParsed = this.TryGetDow(
                            carrierMatch.Groups[1].Value, out DaysOfWeek dow);
                        var carrier = dowParsed ?
                            Regex.Match(g.ToList()[1], @$"([{BCP.AllLetters}\-\s]+)").Groups[1].Value :
                            carrierMatch.Groups[1].Value;
                        var times = dowTimeMatches.Select(m => new
                        {
                            Dow = dowParsed ?
                                dow :
                                this.TryGetDow(m.Groups[2].Value, out dow) ? dow : Constants.AllWeek,
                            Time = m.Groups[1].Value
                        }).ToList();
                        var names = g.First()
                            .Split(new char[] { '–', '-' })
                            .Replace(this.replacements)
                            .Trim()
                            .ToLower()
                            .SkipWhile(n => !n.ToLower().Contains(BCP.City.Bourgas.ToLower()))
                            .ToDictionary(kvp => kvp, kvp => this.regions.GetOrDefault(kvp.ToLower()));
                        var routes = times
                            .Select(t => new Route(
                                BCP.CountryName,
                                t.Dow,
                                carrier,
                                Mode.Bus,
                                Stop.CreateMany(
                                    names.Keys,
                                    t.Time.AsList().AppendMany(null, names.Count() - 1),
                                    regions: names.Values),
                                    url))
                            .Where(r => !this.banned.Contains(r.Carrier))
                            .ToList();

                        allRoutes.AddRange(routes);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, string.Join(" | ", g));
                }
            }

            var legs = new List<Leg>();

            foreach (var route in allRoutes)
            {
                legs.AddRange(await this.routeParser.ParseRouteAsync(route));
            }

            return legs;
        }

        private bool TryGetDow(string s, out DaysOfWeek dow)
        {
            switch (s.Trim().ToLower())
            {
                case "петък, неделя":
                    dow = DaysOfWeek.Friday | DaysOfWeek.Sunday;

                    return true;
                case "делник":
                    dow = Constants.AllWeek | DaysOfWeek.HolidayExclusive;

                    return true;
                case "празник":
                    dow = DaysOfWeek.HolidayInclusive;

                    return true;
                case "от понеделник до събота":
                    dow = Constants.MondayToFriday | DaysOfWeek.Saturday;

                    return true;
                case "събота и неделя":
                    dow = Constants.Weekend;

                    return true;
                default:
                    dow = Constants.AllWeek;

                    return false;
            }
        }
    }
}
