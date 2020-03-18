using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using System;
using System.Threading.Tasks;

namespace Navred.Providers.Bulgaria.Boydevi
{
    public class Function
    {
        private static async Task Main(string[] args)
        {
            Func<string, ILambdaContext, string> func = FunctionHandler;
            using(var handlerWrapper = HandlerWrapper.GetHandlerWrapper(func, new JsonSerializer()))
            using(var bootstrap = new LambdaBootstrap(handlerWrapper))
            {
                await bootstrap.RunAsync();
            }
        }

        public static string FunctionHandler(string input, ILambdaContext context)
        {
            var crawler = new Crawler();
            var task = crawler.GetItinerariesAsync();

            task.Wait();

            var result = task.Result;

            return null;
        }
    }
}
