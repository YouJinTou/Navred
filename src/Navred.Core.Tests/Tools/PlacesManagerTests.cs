using Microsoft.Extensions.DependencyInjection;
using Navred.Core.Configuration;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Places;
using System.Threading.Tasks;
using Xunit;

namespace Navred.Core.Tests.Tools
{
    public class PlacesManagerTests
    {
        [Theory]
        [InlineData(BulgarianCultureProvider.CountryName)]
        public async Task UpdateCoordinatesAsync(string country)
        {
            var provider = new ServiceCollection().AddCore().BuildServiceProvider();
            var placesManager = provider.GetService<IPlacesManager>();
            var configService = provider.GetService<IConfigurationService>();

            configService.TrySetEnvVarsFromLaunchSettings();

            await placesManager.UpdateCoordinatesForCountryAsync<BulgarianPlace>(
                country);
        }
    }
}
