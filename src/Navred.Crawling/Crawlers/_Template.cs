using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core.Abstractions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Models;
using Navred.Core.Processing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Crawling.Crawlers
{
    public class Template : ICrawler
    {
        private const string DeparturesUrl = "URL";
        private const string ArrivalsUrl = "URL";

        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly ILogger<Template> logger;

        public Template(
            IRouteParser routeParser, ILegRepository repo, ILogger<Template> logger)
        {
            this.routeParser = routeParser;
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
            var route = new Route(
                "country", DaysOfWeek.Empty, "carrier", Mode.Bus, Stop.CreateMany(null, null), "info");
            var legs = new List<Leg>();


            return legs;
        }
    }
}
