using HtmlAgilityPack;
using Navred.Core.Abstractions;
using Navred.Core.Itineraries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.SofiaCentralBusStation
{
    public class Crawler : ICrawler
    {
        public async Task<IEnumerable<Leg>> GetLegsAsync()
        {
            var legs = await this.GetLegsAsync("URL");

            return legs;
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
