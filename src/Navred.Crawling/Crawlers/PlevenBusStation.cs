using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core;
using Navred.Core.Abstractions;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
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
        private const string Departures = "https://avtogara.pleven.bg/wp-admin/admin-ajax.php?action=wp_ajax_ninja_tables_public_action&table_id=271&target_action=get-all-data&default_sorting=new_first";
        private const string Arrivals = "https://avtogara.pleven.bg/wp-admin/admin-ajax.php?action=wp_ajax_ninja_tables_public_action&table_id=365&target_action=get-all-data&default_sorting=new_first";

        private readonly IPlacesManager placesManager;
        private readonly ICultureProvider cultureProvider;
        private readonly ILegRepository repo;
        private readonly ILogger<PlevenBusStation> logger;
        private readonly ICollection<string> bannedStops;
        private readonly ICollection<string> stopTrims;
        private readonly IDictionary<string, string> replacements;

        public PlevenBusStation(
            IPlacesManager placesManager, 
            ICultureProvider cultureProvider,
            ILegRepository repo, 
            ILogger<PlevenBusStation> logger)
        {
            this.placesManager = placesManager;
            this.cultureProvider = cultureProvider;
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
                var arrivals = await this.GetLegsAsync<Arrival>(Arrivals);
                var departures = await this.GetLegsAsync<Departure>(Departures);
                var legs = new List<Leg>(arrivals);

                legs.AddRange(departures);

                await repo.UpdateLegsAsync(legs);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Update failed.");
            }
        }

        private async Task<IEnumerable<Leg>> GetLegsAsync<T>(string url) where T : Itinerary
        {
            var web = new HtmlWeb();
            var doc = await web.LoadFromWebAsync(url);
            var decoded = doc.DocumentNode.InnerText.FromUnicode();
            var itineraries = JsonConvert.DeserializeObject<IEnumerable<T>>(decoded);
            var values = itineraries.Select(i => i).ToList();
            var dows = values.Select(v => v.Ref.OnDays).Distinct().ToList();
            var legs = new List<Leg>();

            await values.RunBatchesAsync(30, async (r) =>
            {
                try
                {
                    var v = r.Ref;
                    var stopTimes = this.GetStopTimes(r);
                    var schedule = new Schedule();

                    for (int s = 0; s < stopTimes.Count - 1; s++)
                    {
                        try
                        {
                            var fromStopTime = stopTimes[s];
                            var toStopTime = stopTimes[s + 1];
                            var departureTimes = this.GetDatesAhead(fromStopTime.Item2, v.OnDays);

                            foreach (var departureTime in departureTimes)
                            {
                                var utcArrival = toStopTime.Item2.ToUtcDateTimeDate(departureTime);

                                if (departureTime.Equals(utcArrival))
                                {
                                    continue;
                                }

                                var leg = new Leg(
                                    from: fromStopTime.Item1,
                                    to: toStopTime.Item1,
                                    utcDeparture: departureTime,
                                    utcArrival: utcArrival,
                                    carrier: v.Carrier,
                                    mode: Mode.Bus,
                                    info: url);

                                schedule.AddLeg(leg);
                            }
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, v.Legs);
                        }
                    }

                    legs.AddRange(schedule.GetWithChildren());

                    await Task.CompletedTask;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, r.Ref.Legs);
                }
            });

            return legs;
        }

        private IList<Tuple<Place, TimeSpan>> GetStopTimes<T>(T v) where T : Itinerary
        {
            var legs = v.Ref.Legs.ReplaceTokens(this.stopTrims).ChainReplace(this.replacements);
            var pattern = @$"([\d]{{2}}:[\d]{{2}})\s*?([{BCP.AllLetters}\s]+)";
            var matches = Regex.Matches(legs, pattern);
            var stops = new HashSet<string>();
            var times = new List<TimeSpan>();

            foreach (Match match in matches)
            {
                var stop = match.Groups[2].Value.Trim();
                var time = match.Groups[1].Value;

                if (this.bannedStops.Contains(stop))
                {
                    continue;
                }

                stops.Add(stop);

                times.Add(TimeSpan.Parse(time));
            }

            var placesByKey = 
                this.placesManager.DeducePlacesFromStops(BCP.CountryName, stops.ToList(), false);
            var current = 0;
            var result = new List<Tuple<Place, TimeSpan>>();

            foreach (var kvp in placesByKey)
            {
                if (kvp.Value != null)
                {
                    result.Add(new Tuple<Place, TimeSpan>(kvp.Value, times[current]));
                }

                current++;
            }

            return result;
        }

        private IEnumerable<DateTime> GetDatesAhead(TimeSpan time, string onDays)
        {
            if (onDays.Contains("празни"))
            {
                var qqweq = 5;
            }
            DaysOfWeek dow = (onDays.ToLower()) switch
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
                _ => throw new Exception("Could not determine days of week."),
            };
            var datesAhead = dow.GetValidUtcTimesAhead(
                time, Defaults.DaysAhead, this.cultureProvider.GetHolidays());

            return datesAhead;
        }
    }
}
