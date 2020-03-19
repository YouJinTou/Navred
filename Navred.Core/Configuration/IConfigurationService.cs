using Microsoft.Extensions.Configuration;

namespace Navred.Core.Configuration
{
    public interface IConfigurationService
    {
        IConfiguration GetConfiguration();
    }
}