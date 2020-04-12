using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core;
using Navred.Core.Abstractions;
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
        private readonly ILegRepository repo;
        private readonly ILogger<PlevenBusStation> logger;
        private readonly ICollection<string> bannedStops;
        private readonly ICollection<string> stopTrims;
        private readonly ICollection<string> textElements;

        public PlevenBusStation(
            IPlacesManager placesManager, ILegRepository repo, ILogger<PlevenBusStation> logger)
        {
            this.placesManager = placesManager;
            this.repo = repo;
            this.logger = logger;
            this.bannedStops = new HashSet<string> { "гара" };
            this.stopTrims = new List<string> { "АГ", "Юг", "-", "ч.", "Централна" };
            this.textElements = new List<string> { "</p>", "\n" };
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var arrivals = await this.GetLegsAsync(Arrivals);
                var departures = await this.GetLegsAsync(Departures);

                await repo.UpdateLegsAsync(departures);
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
            var decoded = doc.DocumentNode.InnerText.FromUnicode();
            var itineraries = JsonConvert.DeserializeObject<IEnumerable<Itinerary>>(decoded);
            var values = itineraries.Select(i => i.Value).ToList();
            var legs = new List<Leg>();

            await values.RunBatchesAsync(30, async (v) =>
            {
                try
                {
                    var stopTimes = this.GetStopTimes(v);
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
                    this.logger.LogError(ex, v.Legs);
                }
            });

            return legs;
        }

        private IList<Tuple<Place, TimeSpan>> GetStopTimes(Value v)
        {
            var legs = v.Legs.ReplaceTokens(this.stopTrims).ChainReplace(this.textElements);
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
            var dow = DaysOfWeek.Empty;

            switch (onDays.ToLower())
            {
                case "всички дни":
                    dow = Constants.AllWeek;
                    break;
                case "само петък и неделя":
                    dow = DaysOfWeek.Friday | DaysOfWeek.Sunday;
                    break;
                case "от неделя до петък":
                    dow = DaysOfWeek.Sunday | Constants.MondayToFriday;
                    break;
                case "без събота, неделя и празнични дни":
                    dow = Constants.MondayToFriday;
                    break;
                case "без събота, неделя и понеделник":
                    dow = Constants.MondayToFriday ^ DaysOfWeek.Monday;
                    break;
                case "без събота, неделя и празничен ден":
                    dow = Constants.MondayToFriday;
                    break;
                case "само събота и неделя":
                    dow = Constants.Weekend;
                    break;
                case "от понеделник до събота":
                    dow = Constants.MondayToFriday | DaysOfWeek.Saturday;
                    break;
                case "само събота":
                    dow = DaysOfWeek.Saturday;
                    break;
                case "само неделя":
                    dow = DaysOfWeek.Sunday;
                    break;
                case "от понеделник до петък":
                    dow = Constants.MondayToFriday;
                    break;
                case "понеделник - събота":
                    dow = Constants.MondayToFriday | DaysOfWeek.Saturday;
                    break;
                case "само в неделя":
                    dow = DaysOfWeek.Sunday;
                    break;
                default:
                    throw new Exception("Could not determine days of week.");
            }

            var datesAhead = dow.GetValidUtcTimesAhead(time, Defaults.DaysAhead);

            return datesAhead;
        }
    }
}
