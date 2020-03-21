using Navred.Core.Abstractions;
using Navred.Core.Itineraries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.SofiaCentralBusStation
{
    public class Crawler : ICrawler
    {
        public Task<IEnumerable<Leg>> GetLegsAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
