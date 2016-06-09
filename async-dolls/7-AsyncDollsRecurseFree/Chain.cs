using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsRecurseFree
{
    public class Chain
    {
        readonly List<ILinkElement> executingElements;

        public Chain(IEnumerable<ILinkElement> elements)
        {
            executingElements = new List<ILinkElement>(elements);
        }

        public async Task Invoke(IncomingContext context)
        {
            int currentIndex = 0;
            var semaphore = new SemaphoreSlim(1);
            Stack<Tuple<Task, TaskCompletionSource<ExceptionDispatchInfo>>> sources = new Stack<Tuple<Task, TaskCompletionSource<ExceptionDispatchInfo>>>();

            while (currentIndex < executingElements.Count)
            {
                await semaphore.WaitAsync().ConfigureAwait(false);

                var element = executingElements[currentIndex];
                currentIndex += 1;

                var tcs = new TaskCompletionSource<ExceptionDispatchInfo>();
                var task = element.Invoke(context, () =>
                {
                    semaphore.Release();
                    return tcs.Task;
                });

                sources.Push(Tuple.Create(task, tcs));
            }

            ExceptionDispatchInfo exception = null;
            var anotherSemaphore = new SemaphoreSlim(1);
            var allTasks = new ConcurrentBag<Task>();
            foreach (var source in sources)
            {
                await anotherSemaphore.WaitAsync().ConfigureAwait(false);

                if (exception != null)
                {
                    context.Exceptions.Enqueue(exception);
                    source.Item2.TrySetException(exception.SourceException);
                    exception = null;
                }
                else
                {
                    source.Item2.TrySetResult(null);
                }

                var task = source.Item1.ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        exception = ExceptionDispatchInfo.Capture(t.Exception.InnerException);
                    }
                    anotherSemaphore.Release();
                }, TaskContinuationOptions.ExecuteSynchronously);
                allTasks.Add(task);
            }

            await Task.WhenAll(allTasks).ConfigureAwait(false);
        }
    }
}