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

        public static IList<T> AsList<T>(this T obj)
        {
            return new List<T> { obj };
        }

        public static bool IsAscending<T, TKey>(
            this IEnumerable<T> items, Func<T, TKey> func) where TKey : IComparable<TKey>
        {
            foreach (var (current, next) in items.AsPairs())
            {
                if ((func(current).CompareTo(func(next))) > 0)
                {
                    return false;
                }
            }

            return true;
        }

        public static IList<IEnumerable<T>> AsLists<T>(
            this IEnumerable<T> first, params IEnumerable<T>[] lists)
        {
            var listOfLists = new List<IEnumerable<T>>();

            listOfLists.Add(first);

            listOfLists.AddRange(lists);

            return listOfLists;
        }

        public static IEnumerable<T> InsertBetween<T>(
            this IEnumerable<T> headTail, T value, int totalCount)
        {
            if (headTail.Count() != 2)
            {
                throw new InvalidOperationException("Insertable must contain tail and head only.");
            }

            if (totalCount < 2)
            {
                throw new InvalidOperationException("Total count is less than two.");
            }

            var result = new T[totalCount];
            result[0] = headTail.First();
            result[totalCount - 1] = headTail.Last();

            for (int i = 1; i < totalCount - 1; i++)
            {
                result[i] = value;
            }

            return result;
        }

        public static TValue GetOrDefault<TKey, TValue>(
            this IDictionary<TKey, TValue> dict, TKey key)
        {
            return dict.ContainsKey(key) ? dict[key] : default(TValue);
        }

        public static IEnumerable<T> TakeWhileInclusive<T>(
            this IEnumerable<T> items, Func<T, bool> func)
        {
            var list = items.ToList();
            var result = new List<T>();

            for (int i = 0; i < list.Count; i++)
            {
                result.Add(list[i]);

                if (func(list[i]))
                {
                    break;
                }
            }

            return result;
        }

        public static IEnumerable<T> SkipUntilLast<T>(this IEnumerable<T> items, T target)
        {
            var index = 0;
            var lastIndex = 0;

            foreach (var item in items)
            {
                if (item.Equals(target))
                {
                    lastIndex = index;
                }

                index++;
            }

            return items.Skip(lastIndex);
        }

        public static IEnumerable<IEnumerable<T>> SplitBy<T>(
            this IEnumerable<T> items, Func<T, bool> func)
        {
            var itemsList = items.ToList();
            var result = new List<IEnumerable<T>>();
            var indices = items
                .Select((i, index) => func(i) ? index : -1).Where(i => i != -1).ToList();
            var pairs = indices.AsPairs();

            if (pairs.IsEmpty())
            {
                result.Add(items);

                return result;
            }

            foreach (var (i, j) in pairs)
            {
                result.Add(items.Skip(i).Take(j - i).ToList());
            }

            result.Add(items.Skip(pairs.Last().Item2).ToList());

            return result;
        }

        public static IEnumerable<string> Replace(
            this IEnumerable<string> items, 
            IDictionary<string, string> replacements, 
            bool orderByDescending = true)
        {
            var result = new List<string>();

            foreach (var item in items)
            {
                result.Add(item.ChainReplace(replacements, orderByDescending));
            }

            return result;
        }

        public static IEnumerable<string> Trim(this IEnumerable<string> items)
        {
            var result = new List<string>();

            foreach (var item in items)
            {
                result.Add(item.Trim());
            }

            return result;
        }

        public static IEnumerable<string> ToLower(this IEnumerable<string> items)
        {
            var result = new List<string>();

            foreach (var item in items)
            {
                result.Add(item.ToLower());
            }

            return result;
        }

        public static IEnumerable<T> AppendMany<T>(
            this IEnumerable<T> items, T appendee, int count)
        {
            var result = new List<T>(items);

            for (int i = 0; i < count; i++)
            {
                result.Add(appendee);
            }

            return result;
        }

        public static Dictionary<TKey, TValue> ReverseDict<TKey, TValue>(
            this IDictionary<TKey, TValue> dict)
        {
            var reversedList = dict.Reverse();
            var reversedDict = new Dictionary<TKey, TValue>(reversedList);

            return reversedDict;
        }
    }
}
