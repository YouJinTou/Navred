using Navred.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Abstractions
{
    public interface ICrawler
    {
        Task<IEnumerable<Itinerary>> GetItinerariesAsync();
    }
}
