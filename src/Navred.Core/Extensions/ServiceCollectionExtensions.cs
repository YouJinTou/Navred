using Amazon.DynamoDBv2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Navred.Core.Configuration;
using Navred.Core.Cultures;
using Navred.Core.Search;
using System.Linq;
using System.Reflection;

namespace Navred.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            var temp = services.AddTransient<IConfigurationService, ConfigurationService>()
                .BuildServiceProvider();
            var configService = temp.GetService<IConfigurationService>();
            var config = configService.GetConfiguration();
            var settings = new Settings();

            config.Bind("Settings", settings);

            services
                .AddSingleton(settings)
                .AddDefaultAWSOptions(config.GetAWSOptions())
                .AddAWSService<IAmazonDynamoDB>()
                .AddByConvention(typeof(Constants).Assembly)
                .AddTransient<ICultureProvider, BulgarianCultureProvider>();

            return services;
        }

        public static IServiceCollection AddByConvention(
            this IServiceCollection services, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var serviceItems = typeof(Constants).Assembly.GetTypes()
                    .Where(t =>
                        t.IsClass &&
                        !t.IsAbstract &&
                        !t.IsGenericType &&
                        t.GetInterfaces().Any(i => $"I{t.Name}" == i.Name))
                    .Select(t => new
                    {
                        Implementation = t,
                        Interface = t.GetInterface($"I{t.Name}")
                    }).ToList();

                foreach (var service in serviceItems)
                {
                    services.AddTransient(service.Interface, service.Implementation);
                }
            }

            return services;
        }
    }
}
