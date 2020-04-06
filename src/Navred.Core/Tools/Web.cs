using System;
using System.Threading.Tasks;

namespace Navred.Core.Tools
{
    public class Web
    {
        public async Task WithBackoffAsync(
            Func<Task> func, int maximumBackoffSeconds = 64, int maxRetries = 7)
        {
            var random = new Random();
            var maxMilliseconds = maximumBackoffSeconds * 1000;

            for (int n = 0; n <= maxRetries; n++)
            {
                try
                {
                    await func();

                    return;
                }
                catch (Exception ex)
                {
                    if (n + 1 > maxRetries)
                    {
                        throw;
                    }

                    Console.WriteLine($"Retrying failed ({n + 1}): {ex}");

                    var sleepTime = (int)Math.Min(
                        Math.Pow(2, n) * 1000 + random.NextDouble() * 1000, maxMilliseconds);

                    Console.WriteLine($"Sleeping for: {sleepTime}");

                    await Task.Delay(sleepTime);
                }
            }
        }
    }
}
