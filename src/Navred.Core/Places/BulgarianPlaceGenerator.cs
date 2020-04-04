using Navred.Core.Cultures;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Navred.Core.Places
{
    public class BulgarianPlaceGenerator : IBulgarianPlaceGenerator
    {
        private class Ekatte
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Region { get; set; }

            public string Municipality { get; set; }
        }

        public void GeneratePlaces()
        {
            var placeEkattes = this.GetPlaceEkattes();
            var municipalityEkattes = this.GetMunicipalityEkattes();
            var places = new List<Place>();

            foreach (var placeEkatte in placeEkattes)
            {
                var municipality = municipalityEkattes.First(
                    me => me.Id == placeEkatte.Municipality);
                var place = new Place
                {
                    Country = BulgarianCultureProvider.CountryName,
                    Name = placeEkatte.Name,
                    Region = BulgarianCultureProvider.Region.NameByCode[placeEkatte.Region],
                    Municipality = municipality.Name
                };

                places.Add(place);
            }

            var placesString = JsonConvert.SerializeObject(places);

            File.WriteAllText(
                $"Resources/{BulgarianCultureProvider.CountryName}_places.json", placesString);
        }

        private IEnumerable<Ekatte> GetPlaceEkattes()
        {
            var lines = File.ReadAllText("Resources/bulgaria_ekatte.csv")
                .Split("\r\n")
                .Skip(2)
                .ToList();
            var ekattes = new List<Ekatte>();

            foreach (var line in lines)
            {
                var tokens = line.Split(',').ToList();
                var ekatte = new Ekatte
                {
                    Id = tokens[0],
                    Name = tokens[2],
                    Region = tokens[3],
                    Municipality = tokens[4]
                };

                ekattes.Add(ekatte);
            }

            return ekattes;
        }

        private IEnumerable<Ekatte> GetMunicipalityEkattes()
        {
            var lines = File.ReadAllText("Resources/bulgaria_ekatte_obst.csv")
                .Split("\r\n")
                .Skip(1)
                .ToList();
            var ekattes = new List<Ekatte>();

            foreach (var line in lines)
            {
                var tokens = line.Split(',').ToList();
                var ekatte = new Ekatte
                {
                    Id = tokens[0],
                    Name = tokens[2],
                    Region = tokens[1]
                };

                ekattes.Add(ekatte);
            }

            return ekattes;
        }
    }
}
