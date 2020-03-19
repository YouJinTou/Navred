using Microsoft.Extensions.Configuration;
using Navred.Core.Tools;
using System.IO;

namespace Navred.Core.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        public IConfiguration GetConfiguration()
        {
            var stage = Env.GetVar(Constants.StageUpper);
            var env = string.IsNullOrWhiteSpace(stage) ?
                Env.GetVar("ASPNETCORE_ENVIRONMENT", "Development") : 
                (stage == "prod") ? "Production" : "Development";
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }
}
