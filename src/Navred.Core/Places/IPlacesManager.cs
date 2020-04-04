using System.Collections.Generic;
using System.Threading.Tasks;

namespace Navred.Core.Places
{
    public interface IPlacesManager
    {
        void GeneratePlacesFor(string country);

        IEnumerable<Place> LoadPlacesFor(string country);

        Task<IEnumerable<Place>> UpdateCoordinatesForCountryAsync(string country);

        void UpdatePlacesFor(string country, IEnumerable<Place> places);

        string FormatPlace(string place);

        Place GetPlace(string id);

        Place GetPlace(
            string country, 
            string name, 
            string regionCode = null, 
            string municipalityCode = null,
            IEnumerable<string> neighbors = null);

        string NormalizePlaceName(
            string country, 
            string name, 
            string regionCode = null, 
            string municipalityCode = null,
            IEnumerable<string> neighbors = null);
    }
}