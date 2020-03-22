using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Places
{
    public interface IPlacesManager
    {
        void GeneratePlacesFor(string country);

        IEnumerable<T> LoadPlacesFor<T>(string country) where T : IPlace;

        Task<IEnumerable<T>> UpdateCoordinatesForCountryAsync<T>(string country) where T : IPlace;

        void UpdatePlacesFor<T>(string country, IEnumerable<T> places) where T : IPlace;

        T GetPlace<T>(
            string country, 
            string name, 
            string regionCode = null, 
            string municipalityCode = null) where T : IPlace;
    }
}