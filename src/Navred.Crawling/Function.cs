using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Navred.Core.Abstractions;
using System;
using System.Threading.Tasks;

namespace Navred.Crawling
{
    public class Function
    {
        private static ICrawler crawler;

        private static async Task Main(string[] args)
        {
            Action<string, ILambdaContext> func = HandleRequest;
            using var wrapper = HandlerWrapper.GetHandlerWrapper(func, new JsonSerializer());
            using var bootstrap = new LambdaBootstrap(wrapper);

            await bootstrap.RunAsync();
        }

        public static void SetCrawler(ICrawler crawler)
        {
            Function.crawler = crawler;
        }

        public static void HandleRequest(string input, ILambdaContext context)
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
