using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Itineraries.DB
{
    public interface ILegRepository
    {
        Task<IEnumerable<Leg>> GetLegsAsync(string from, string to, TimeWindow window);

        Task UpdateLegsAsync(IEnumerable<Leg> itineraries);
    }
}