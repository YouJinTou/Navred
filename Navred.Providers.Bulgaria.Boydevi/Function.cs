using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using Navred.Core.Cultures;
using Navred.Core.Extensions;
using Navred.Core.Itineraries.DB;
using System;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.Boydevi
{
    public class Function
    {
        private static async Task Main(string[] args)
        {
            Action<string, ILambdaContext> func = FunctionHandler;

            using(var handlerWrapper = HandlerWrapper.GetHandlerWrapper(func, new JsonSerializer()))
            using(var bootstrap = new LambdaBootstrap(handlerWrapper))
            {
                await bootstrap.RunAsync();
            }
        }

        public static void FunctionHandler(string input, ILambdaContext context)
        {
            var provider = new ServiceCollection().AddCore().BuildServiceProvider();
            var repo = provider.GetService<IItineraryRepository>();
            var cultureProvider = provider.GetService<IBulgarianCultureProvider>();
            var crawler = new Crawler(repo, cultureProvider);
            var task = crawler.GetItinerariesAsync();

            task.Wait();
        }
    }
}
