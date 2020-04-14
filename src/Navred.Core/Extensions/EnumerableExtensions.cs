using Navred.Core.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Navred.Core.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Count() == 0;
        }

        public static bool IsEmpty(this System.Collections.IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Count() == 0;
        }

        public static bool ContainsOne<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Count() == 1;
        }

        public static bool IsNullOrEmpty(this System.Collections.IEnumerable enumerable)
        {
            if (enumerable == null || IsEmpty(enumerable))
            {
                return true;
            }

            return false;
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null || IsEmpty(enumerable))
            {
                return true;
            }

            return false;
        }

        public static System.Collections.IEnumerable ToGenericEnumerable(
            this System.Collections.IEnumerable enumerable, Type type)
        {
            var genericList = typeof(List<>).MakeGenericType(type);
            var list = Activator.CreateInstance(genericList) as System.Collections.IList;

            foreach (var item in enumerable)
            {
                list.Add(item);
            }

            return list;
        }

        public static IEnumerable<IEnumerable<T>> ToBatches<T>(
            this IEnumerable<T> enumerable, int batchSize)
        {
            var remainder = enumerable.Count() % batchSize;
            var finalBatch = remainder > 0 ? 1 : 0;
            var totalBatches = (enumerable.Count() / batchSize) + finalBatch;
            var batches = new List<IEnumerable<T>>(totalBatches);

            for (int b = 0; b < totalBatches; b++)
            {
                batches.Add(new List<T>(enumerable.Skip(b * batchSize).Take(batchSize).ToList()));
            }

            return batches;
        }

        public static async Task RunBatchesAsync<T>(
            this IEnumerable<T> enumerable,
            int batchSize,
            Func<T, Task> run,
            int delayBetweenBatches = 0,
            int delayBetweenBatchItems = 0,
            bool withRetry = true,
            int maximumBackoffSeconds = 64,
            int maxRetries = 7)
        {
            var batches = ToBatches(enumerable, batchSize);

            foreach (var batch in batches)
            {
                var tasks = new List<Task>();

                foreach (var item in batch)
                {
                    if (withRetry)
                    {
                        var t = new Web().WithBackoffAsync(
                            async () => await run(item), maximumBackoffSeconds, maxRetries);

                        tasks.Add(t);
                    }
                    else
                    {
                        tasks.Add(run(item));
                    }

                    await Task.Delay(delayBetweenBatchItems);
                }

                await Task.WhenAll(tasks);

                await Task.Delay(delayBetweenBatches);
            }
        }

        public static IEnumerable<T> TakeAllButLast<T>(this IEnumerable<T> enumerable, int last)
        {
            var count = enumerable.Count();

            if (count < last)
            {
                return enumerable.Take(0);
            }

            return enumerable.Take(count - last);
        }

        public static T GetMin<T, TC>(this IEnumerable<T> enumerable, Func<T, TC> func) where TC : IComparable<TC>
        {
            var currentMin = func(enumerable.First());
            var currentMinItem = enumerable.First();

            foreach (var item in enumerable)
            {
                var result = func(item);

                if (currentMin.CompareTo(result) > 0)
                {
                    currentMin = result;
                    currentMinItem = item;
                }
            }

            return currentMinItem;
        }

        public static IEnumerable<(T, T)> AsPairs<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable.ToList();

            for (int i = 0; i < list.Count - 1; i++)
            {
                yield return (list[i], list[i + 1]);
            }
        }

        public static string GetOrReturn(
            this IDictionary<string, string> dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key];
            }

            return key;
        }

        public static IEnumerable<T> AsList<T>(this T obj)
        {
            return new List<T> { obj };
        }
    }
}
