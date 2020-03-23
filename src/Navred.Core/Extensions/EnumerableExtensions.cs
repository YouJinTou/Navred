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

        public static bool ContainsOne<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Count() == 1;
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
            int delayBetweenBatchItems = 0)
        {
            var batches = ToBatches(enumerable, batchSize);

            foreach (var batch in batches)
            {
                var tasks = new List<Task>();

                foreach (var item in batch)
                {
                    var t = run(item);

                    tasks.Add(t);

                    await Task.Delay(delayBetweenBatchItems);
                }

                await Task.WhenAll(tasks);

                await Task.Delay(delayBetweenBatches);
            }
        }
    }
}
