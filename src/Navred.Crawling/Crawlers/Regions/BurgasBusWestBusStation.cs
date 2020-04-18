using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using Navred.Core.Models;
using Navred.Core.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BCP = Navred.Core.Cultures.BulgarianCultureProvider;

namespace Navred.Crawling.Crawlers.Regions
{
    public class BurgasBusWestBusStation : ICrawler
    {
        private const string Url = "http://burgasbus.info/burgasbus/?page_id=69";

        private readonly IRouteParser routeParser;
        private readonly ILegRepository repo;
        private readonly ILogger<BurgasBusWestBusStation> logger;

        public BurgasBusWestBusStation(
            IRouteParser routeParser, ILegRepository repo, ILogger<BurgasBusWestBusStation> logger)
        {
            this.routeParser = routeParser;
            this.repo = repo;
            this.logger = logger;
        }

        public async Task UpdateLegsAsync()
        {
            try
            {
                var legs = await this.GetLegsAsync(Url);
                var all = new List<Leg>(legs);

                await repo.UpdateLegsAsync(all);
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
            var ps = doc.DocumentNode.SelectNodes("//p")
                .Select(p => p.InnerText.Trim())
                .SkipWhile(s => !s.ToLower().Contains("сектор"))
                .ToList();
            var groups = ps.SplitBy(s => s.ToLower().Contains("линия"));

            foreach (var g in groups)
            {

            }
            var route = new Route(
                BCP.CountryName, 
                DaysOfWeek.Empty, 
                "carrier", 
                Mode.Bus, 
                Stop.CreateMany(null, null), 
                url);
            var legs = new List<Leg>();


            return legs;
        }
    }
}
