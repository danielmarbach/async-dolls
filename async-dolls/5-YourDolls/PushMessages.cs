using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AsyncDolls.YourDolls
{
    public class PushMessages : IPushMessages
    {
        Func<TransportMessage, Task> onMessageAsync;
        private readonly ConcurrentQueue<TransportMessage> messages;
        private ConcurrentDictionary<Task, Task> runningTasks;
        private SemaphoreSlim semaphore;
        private CancellationTokenSource tokenSource;
        private Task pumpTask;
        private readonly int maxConcurrency;

        public PushMessages(ConcurrentQueue<TransportMessage> messages, int maxConcurrency = 100)
        {
            this.maxConcurrency = maxConcurrency;
            this.messages = messages;
        }

        public Task StartAsync(Func<TransportMessage, Task> onMessage)
        {
            onMessageAsync = onMessage;

            runningTasks = new ConcurrentDictionary<Task, Task>();
            semaphore = new SemaphoreSlim(maxConcurrency);
            tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;

            pumpTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await semaphore.WaitAsync(token).ConfigureAwait(false);

                    TransportMessage transportMessage;
                    if (messages.TryDequeue(out transportMessage))
                    {
                        var task = onMessageAsync(transportMessage);

                        runningTasks.TryAdd(task, task);

                        task.ContinueWith(t =>
                        {
                            semaphore.Release();
                            Task taskToBeRemoved;
                            runningTasks.TryRemove(t, out taskToBeRemoved);
                        }, TaskContinuationOptions.ExecuteSynchronously)
                            .Ignore();
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(20), token).ConfigureAwait(false);
                    }

                }
            }, token);

            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            tokenSource.Cancel();

            await pumpTask.IgnoreCancellation().ConfigureAwait(false);
            await Task.WhenAll(runningTasks.Values).ConfigureAwait(false);

            runningTasks.Clear();
            semaphore.Dispose();
            tokenSource.Dispose();
        }
    }
}