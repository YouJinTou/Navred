using HtmlAgilityPack;
using Navred.Core.Abstractions;
using Navred.Core.Itineraries;
using Navred.Core.Itineraries.DB;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.SofiaCentralBusStation
{
    public class Crawler : ICrawler
    {
        private readonly ILegRepository repo;

        public Crawler(ILegRepository repo)
        {
            this.repo = repo;
        }

        public async Task UpdateLegsAsync()
        {
            var legs = await this.GetLegsAsync("https://www.centralnaavtogara.bg/index.php#b");

            await this.repo.UpdateLegsAsync(legs);
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
