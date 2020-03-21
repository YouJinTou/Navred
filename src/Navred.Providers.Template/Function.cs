using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using Navred.Core.Extensions;
using System;
using System.Threading.Tasks;

namespace Navred.Providers.Template
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
        }
    }
}
