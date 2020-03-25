using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using Navred.Core.Abstractions;
using Navred.Core.Extensions;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.VelikoTarnovoSouthBusStation
{
    public class Function
    {
        private static readonly ICrawler crawler;

        static Function()
        {
            var provider = new ServiceCollection()
                .AddCore()
                .AddTransient<ICrawler, Crawler>()
                .BuildServiceProvider();
            crawler = provider.GetService<ICrawler>();
        }

        private static async Task Main(string[] args)
        {
            if (Debugger.IsAttached)
            {
                crawler.UpdateLegsAsync().Wait();
            }
            else
            {
                Action<string, ILambdaContext> func = FunctionHandler;

                using (var handlerWrapper = HandlerWrapper.GetHandlerWrapper(func, new JsonSerializer()))
                using (var bootstrap = new LambdaBootstrap(handlerWrapper))
                {
                    await bootstrap.RunAsync();
                }
            }
        }

        public static void FunctionHandler(string input, ILambdaContext context)
        {
            try
            {
                var task = crawler.UpdateLegsAsync();

                task.Wait();
            }
            catch (Exception ex)
            {
                context.Logger.Log(ex.ToString());
            }
        }
    }
}
