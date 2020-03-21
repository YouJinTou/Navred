using System.Collections.Generic;
using System.Threading.Tasks;
using Navred.Core.Abstractions;
using Navred.Core.Itineraries;

namespace Navred.Providers.Template
{
    public class Crawler : ICrawler
    {
        public Task<IEnumerable<Leg>> GetLegsAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}
