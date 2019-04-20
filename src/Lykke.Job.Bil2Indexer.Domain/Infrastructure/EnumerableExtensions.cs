using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Infrastructure
{
    public static class EnumerableExtensions
    {
        public static async Task<IReadOnlyCollection<TResult>> MapAsync<TSource, TResult>(
            this IEnumerable<TSource> source,
            int degreeOfParallelism,
            Func<TSource, Task<TResult>> body)
        {
            var tasks = new List<Task<TResult>>();

            using (var throttler = new SemaphoreSlim(degreeOfParallelism))
            {
                foreach (var element in source)
                {
                    await throttler.WaitAsync();

                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            return body(element);
                        }
                        finally
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            throttler.Release();
                        }
                    }));
                }

                var result = await Task.WhenAll(tasks);

                return result;
            }
        }

        public static async Task ForEachAsync<T>(
            this IEnumerable<T> source,
            int degreeOfParallelism,
            Func<T, Task> body)
        {
            var tasks = new List<Task>();

            using (var throttler = new SemaphoreSlim(degreeOfParallelism))
            {
                foreach (var element in source)
                {
                    await throttler.WaitAsync();

                    tasks.Add(Task.Run(() =>
                    {
                        try
                        {
                            return body(element);
                        }
                        finally
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);
            }
        }

        public static async Task<bool> ForEachAsync<T>(
            this IEnumerable<T> source,
            int degreeOfParallelism,
            Func<T, Task<bool>> body)
        {
            var tasks = new List<Task>();

            using (var throttler = new SemaphoreSlim(degreeOfParallelism))
            {
                var haveToAbort = false;

                foreach (var element in source)
                {
                    await throttler.WaitAsync();

                    if (haveToAbort)
                    {
                        break;
                    }

                    tasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            if (!await body(element))
                            {
                                haveToAbort = true;
                            }
                        }
                        finally
                        {
                            // ReSharper disable once AccessToDisposedClosure
                            throttler.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);

                return !haveToAbort;
            }
        }
    }
}
