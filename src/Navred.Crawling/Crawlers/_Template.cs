using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core.Abstractions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Places;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Crawling.Crawlers
{
    public class Template : ICrawler
    {
        private const string DeparturesUrl = "URL";
        private const string ArrivalsUrl = "URL";

        private readonly IPlacesManager placesManager;
        private readonly ILegRepository repo;
        private readonly ILogger<Template> logger;

        public Template(
            IPlacesManager placesManager, ILegRepository repo, ILogger<Template> logger)
        {
            this.placesManager = placesManager;
            this.repo = repo;
            this.logger = logger;
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var departures = await this.GetLegsAsync(DeparturesUrl);
                var arrivals = await this.GetLegsAsync(ArrivalsUrl);
                var legs = new List<Leg>(departures);

                legs.AddRange(arrivals);

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
            var legs = new List<Leg>();


            return legs;
        }
    }
}
