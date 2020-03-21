using Microsoft.Extensions.Configuration;
using Navred.Core.Tools;
using Newtonsoft.Json.Linq;
using System;
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

        public void TrySetEnvVarsFromLaunchSettings()
        {
            var path = Path.Combine(
                Directory.GetCurrentDirectory(), "Properties/launchSettings.json");

            if (!File.Exists(path))
            {
                return;
            }

            var settings = JObject.Parse(File.ReadAllText(path));
            var vars = settings["profiles"]["Navred.Core.Tests"]["environmentVariables"];

            foreach (var envVar in vars)
            {
                var prop = (JProperty)envVar;

                Environment.SetEnvironmentVariable(prop.Name, prop.Value.ToString());
            }
        }
    }
}
