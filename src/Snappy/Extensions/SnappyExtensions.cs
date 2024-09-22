using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Snappy.Extensions
{

    using Snappy.Services;

    public static class SnappyExtensions
    {
        public static void AddSnapcastDependencies(this IServiceCollection services)
        {
            services.AddHttpClient<ISnapcastService, SnapcastService>("SnapcastService", c =>
            {
                c.BaseAddress = new Uri("http://snapserver-direct.iszland.com:1780/jsonrpc");
                c.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5));
        }

        public static async Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body, AsyncParallelOptions asyncParallelOptions)
        {
            ConcurrentQueue<Exception> exceptions = new ConcurrentQueue<Exception>();

            var maxDegreeOfConcurrency = asyncParallelOptions.MaxDegreeOfParallelism;

            var cts = CancellationTokenSource.CreateLinkedTokenSource(asyncParallelOptions.CancellationToken);

            var allDone = Task.WhenAll(
                from partition in Partitioner.Create(source).GetPartitions(maxDegreeOfConcurrency)
                select Task.Run(async delegate
                {
                    using (partition)
                    {
                        while (true)
                        {
                            cts.Token.ThrowIfCancellationRequested();
                            if (!partition.MoveNext())
                            {
                                break;
                            }

                            await body(partition.Current).ContinueWith(t =>
                            {
                                if (t.Exception != null)
                                {
                                    foreach(var ex in t.Exception.Flatten().InnerExceptions)
                                    {
                                        exceptions.Enqueue(ex);
                                    }

                                    if (asyncParallelOptions.FailImmediately)
                                    {
                                        cts.Cancel();
                                    }
                                }
                            });
                        }
                    }
                }, cts.Token));

            await allDone;

            if(exceptions.Count() > 0)
            {
                throw new AggregateException(exceptions);
            }

            if(cts.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
        }

        public class AsyncParallelOptions : System.Threading.Tasks.ParallelOptions
        {
            public bool FailImmediately { get; set; } = true;
        }
    }
}