using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Models;
using Navred.Core.Processing;
using Navred.Crawling.Models.PlevenBusStation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers
{
    public class PlevenBusStation : ICrawler
    {
        private const string DeparturesUrl = "https://avtogara.pleven.bg/wp-admin/admin-ajax.php?action=wp_ajax_ninja_tables_public_action&table_id=271&target_action=get-all-data&default_sorting=new_first";
        private const string ArrivalsUrl = "https://avtogara.pleven.bg/wp-admin/admin-ajax.php?action=wp_ajax_ninja_tables_public_action&table_id=365&target_action=get-all-data&default_sorting=new_first";

        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly ILogger<PlevenBusStation> logger;
        private readonly ICollection<string> bannedStops;
        private readonly ICollection<string> stopTrims;
        private readonly IDictionary<string, string> replacements;

        public PlevenBusStation(
            IRouteParser routeParser, ILegRepository repo, ILogger<PlevenBusStation> logger)
        {
            this.routeParser = routeParser;
            this.repo = repo;
            this.logger = logger;
            this.bannedStops = new HashSet<string> { "гара" };
            this.stopTrims = new List<string> { "АГ", "Юг", "-", "ч.", "Централна" };
            this.replacements = new Dictionary<string, string>
            {
                { "оряховица", "ореховица" },
                { "</p>", string.Empty },
                {  "\n", string.Empty }
            };
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var arrivals = await this.GetLegsAsync<Arrival>(ArrivalsUrl);
                var departures = await this.GetLegsAsync<Departure>(DeparturesUrl);
                var legs = new List<Leg>(arrivals);

                legs.AddRange(departures);

                await repo.UpdateLegsAsync(legs);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync<T>(string url) where T : Models.PlevenBusStation.Itinerary
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var decoded = doc.DocumentNode.InnerText.FromUnicode();
            var itineraries = JsonConvert.DeserializeObject<IEnumerable<T>>(decoded);
            var values = itineraries.Select(i => i).ToList();
            var all = new List<Leg>();

            await values.RunBatchesAsync(30, async (r) =>
            {
                try
                {
                    var v = r.Ref;
                    var dow = this.GetDow(v.OnDays);
                    var legString = v.Legs
                        .ReplaceTokens(this.stopTrims)
                        .ChainReplace(this.replacements);
                    var pattern = @$"([\d]{{2}}:[\d]{{2}})\s*?([{BCP.AllLetters}\s]+)";
                    var matches = Regex.Matches(legString, pattern);
                    var names = matches.Select(m => m.Groups[2].Value.Trim());
                    var times = matches.Select(m => m.Groups[1].Value);
                    var stops = Stop.CreateMany(names, times);
                    var route = new Route(BCP.CountryName, dow, v.Carrier, Mode.Bus, stops, url);
                    var legs = await this.routeParser.ParseRouteAsync(route);

                    all.AddRange(legs);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, r.Ref.Legs);
                }
            });

            return all;
        }

        private DaysOfWeek GetDow(string onDays)
        {
            return onDays switch
            {
                "всички дни" => Constants.AllWeek,
                "всеки ден" => Constants.AllWeek,
                "само петък и неделя" => DaysOfWeek.Friday | DaysOfWeek.Sunday,
                "от неделя до петък" => DaysOfWeek.Sunday | Constants.MondayToFriday,
                "без събота, неделя и празнични дни" => Constants.MondayToFriday | DaysOfWeek.HolidayExclusive,
                "без събота, неделя и понеделник" => Constants.MondayToFriday ^ DaysOfWeek.Monday,
                "без събота, неделя и празничен ден" => Constants.MondayToFriday | DaysOfWeek.HolidayExclusive,
                "само събота и неделя" => Constants.Weekend,
                "от понеделник до събота" => Constants.MondayToFriday | DaysOfWeek.Saturday,
                "само събота" => DaysOfWeek.Saturday,
                "само неделя" => DaysOfWeek.Sunday,
                "от понеделник до петък" => Constants.MondayToFriday,
                "понеделник - събота" => Constants.MondayToFriday | DaysOfWeek.Saturday,
                "понеделник - петък" => Constants.MondayToFriday,
                "само в неделя" => DaysOfWeek.Sunday,
                "без неделя" => Constants.AllWeek ^ DaysOfWeek.Sunday,
                _ => throw new Exception("Could not determine days of week.")
            };
        }
    }
}
