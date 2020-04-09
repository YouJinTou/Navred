using Navred.Core.Places;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries.DB
{
    public interface ILegRepository
    {
        Task<IEnumerable<Leg>> GetLegsAsync(Place from, Place to, TimeWindow window);

        Task UpdateLegsAsync(IEnumerable<Leg> itineraries);

        Task DeleteAllLegsAsync();
    }
}