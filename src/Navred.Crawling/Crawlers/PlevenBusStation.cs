using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
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
        private const string Url = "https://avtogara.pleven.bg/wp-admin/admin-ajax.php?action=wp_ajax_ninja_tables_public_action&table_id=271&target_action=get-all-data&default_sorting=new_first";

        private readonly IPlacesManager placesManager;
        private readonly ILegRepository repo;
        private readonly ILogger<PlevenBusStation> logger;
        private readonly ICollection<string> bannedStops;

        public PlevenBusStation(
            IPlacesManager placesManager, ILegRepository repo, ILogger<PlevenBusStation> logger)
        {
            this.placesManager = placesManager;
            this.repo = repo;
            this.logger = logger;
            this.bannedStops = new HashSet<string> { "гара" };
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
            var decoded = doc.DocumentNode.InnerText.FromUnicode();
            var itineraries = JsonConvert.DeserializeObject<IEnumerable<Itinerary>>(decoded);
            var values = itineraries.Select(i => i.Value).ToList();
            var legs = new List<Leg>();

            foreach (var v in values)
            {
                var placesByKey = this.GetPlacesByKey(v);
                var fromTo = this.GetFromTo(v, placesByKey);
                //var departure = 
                //var leg = new Leg(
                //    from: fromTo.Item1,
                //    to: fromTo.Item2
            }

            return legs;
        }

        private IDictionary<string, Place> GetPlacesByKey(Value v)
        {
            var pattern = $"([{BCP.AllLetters}]+)";
            var matches = Regex.Matches(v.Legs, pattern);
            var stops = matches.Select(m => m.Groups[1].Value)
                .Where(s => !this.bannedStops.Contains(s)).ToList();
            var placesByKey = this.placesManager.DeducePlacesFromStops(BCP.CountryName, stops);

            return placesByKey;
        }

        private (Place, Place) GetFromTo(Value v, IDictionary<string, Place> placesByKey)
        {
            var pattern = $"([{BCP.AllLetters}]+) - ([{BCP.AllLetters}]+)";
            var match = Regex.Match(v.FromToWithTime, pattern);
            var from = placesByKey[match.Groups[1].Value];
            var to = placesByKey[match.Groups[2].Value];

            return (from, to);
        }
    }
}
