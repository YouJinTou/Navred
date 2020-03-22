using Navred.Core.Cultures;
using System;

namespace Navred.Core.Places
{
    public class PlaceGeneratorFactory : IPlaceGeneratorFactory
    {
        private readonly IBulgarianPlaceGenerator bulgarianPlaceGenerator;

        public PlaceGeneratorFactory(IBulgarianPlaceGenerator bulgarianPlaceGenerator)
        {
            this.bulgarianPlaceGenerator = bulgarianPlaceGenerator;
        }

        public IPlaceGenerator CreateGenerator(string country)
        {
            switch (country)
            {
                case BulgarianCultureProvider.CountryName:
                    return this.bulgarianPlaceGenerator;
                default:
                    throw new NotImplementedException(country);
            }
        }
    }
}
