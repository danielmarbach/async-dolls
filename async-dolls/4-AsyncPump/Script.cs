using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
// ReSharper disable AccessToDisposedClosure
// ReSharper disable MethodSupportsCancellation

namespace AsyncDolls
{
    [TestFixture]
    public class AsyncPumpScript
    {
        [Test]
        public async Task ThePump()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            var token = tokenSource.Token;

            var pumpTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    #region Output

                    "Pumping...".Output();

                    #endregion

                    await HandleMessage().ConfigureAwait(false);
                }
            });

            await pumpTask.ConfigureAwait(false);

            tokenSource.Dispose();
        }

        static Task HandleMessage()
        {
            return Task.Delay(1000);
        }

        [Test]
        public async Task CaveatsOfTaskFactoryStartNew()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            var token = tokenSource.Token;

            Task<Task> pumpTask = Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    #region Output

                    "Pumping...".Output();

                    #endregion

                    await HandleMessage().ConfigureAwait(false);
                }
            }, TaskCreationOptions.LongRunning);

            await pumpTask.Unwrap().ConfigureAwait(false);

            tokenSource.Dispose();
        }

        // http://referencesource.microsoft.com/#mscorlib/system/threading/Tasks/ThreadPoolTaskScheduler.cs,57

        [Test]
        public async Task ConcurrentlyHandleMessages()
        {
            #region Cancellation AsAbove

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            var token = tokenSource.Token;

            #endregion

            var runningTasks = new ConcurrentDictionary<Task, Task>();

            var pumpTask = Task.Run(() =>
            {
                while (!token.IsCancellationRequested)
                {
                    #region Output

                    "Pumping...".Output();

                    #endregion

                    var runningTask = HandleMessage();

                    runningTasks.TryAdd(runningTask, runningTask);

                    runningTask.ContinueWith(t =>
                    {
                        #region Output

                        "... done".Output();

                        #endregion

                        Task taskToBeRemoved;
                        runningTasks.TryRemove(t, out taskToBeRemoved);
                    }, TaskContinuationOptions.ExecuteSynchronously);
                }
            });

            await pumpTask.ConfigureAwait(false);

            #region Output

            "Pump finished".Output();

            #endregion

            await Task.WhenAll(runningTasks.Values).ConfigureAwait(false);

            #region Output

            "All receives finished".Output();

            #endregion

            tokenSource.Dispose();
        }

        [Test]
        public async Task LimitingConcurrency()
        {
            #region Cancellation AsAbove

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            var token = tokenSource.Token;

            #endregion

            #region Task Tracking AsAbove

            var runningTasks = new ConcurrentDictionary<Task, Task>();

            #endregion

            var semaphore = new SemaphoreSlim(2);

            var pumpTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    #region Output

                    "Pumping...".Output();

                    #endregion

                    await semaphore.WaitAsync().ConfigureAwait(false);

                    #region HandleMessage AsAbove

                    var runningTask = HandleMessage();

                    runningTasks.TryAdd(runningTask, runningTask);

                    #endregion

                    runningTask.ContinueWith(t =>
                    {
                        #region Output

                        "... done".Output();

                        #endregion

                        semaphore.Release();

                        #region Housekeeping AsAbove

                        Task taskToBeRemoved;
                        runningTasks.TryRemove(t, out taskToBeRemoved);

                        #endregion

                    }, TaskContinuationOptions.ExecuteSynchronously)
                    .Ignore();
                }
            });

            #region Awaiting completion AsAbove
            await pumpTask.ConfigureAwait(false);

            #region Output

            "Pump finished".Output();

            #endregion

            await Task.WhenAll(runningTasks.Values).ConfigureAwait(false);

            #region Output

            "All receives finished".Output();

            #endregion

            tokenSource.Dispose();
            semaphore.Dispose();

            #endregion
        }

        [Test]
        public async Task CancellingAndGracefulShutdown()
        {
            #region Cancellation AsAbove

            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(1));
            var token = tokenSource.Token;

            #endregion

            #region Task Tracking AsAbove

            var runningTasks = new ConcurrentDictionary<Task, Task>();

            #endregion

            #region Limiting AsAbove

            var semaphore = new SemaphoreSlim(2);

            #endregion

            var pumpTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    #region Output

                    "Pumping...".Output();

                    #endregion

                    await semaphore.WaitAsync(token).ConfigureAwait(false);

                    #region HandleMessage AsAbove

                    var runningTask = HandleMessageWithCancellation(token);

                    runningTasks.TryAdd(runningTask, runningTask);

                    #endregion

                    #region Releasing Semaphore & Housekeeping AsAbove

                    runningTask.ContinueWith(t =>
                    {
                        #region Output

                        "... done".Output();

                        #endregion

                        semaphore.Release();

                        #region Housekeeping

                        Task taskToBeRemoved;
                        runningTasks.TryRemove(t, out taskToBeRemoved);

                        #endregion

                    }, TaskContinuationOptions.ExecuteSynchronously)
                        .Ignore();

                    #endregion
                }
            }, CancellationToken.None);

            await pumpTask.IgnoreCancellation().ConfigureAwait(false);

            #region Awaiting completion

            #region Output

            "Pump finished".Output();

            #endregion

            await Task.WhenAll(runningTasks.Values).IgnoreCancellation().ConfigureAwait(false);

            #region Output

            "All receives finished".Output();

            #endregion

            tokenSource.Dispose();
            semaphore.Dispose();

            #endregion
        }

        static Task HandleMessageWithCancellation(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Delay(1000, cancellationToken);
        }

        [Test]
        public async Task TheCompletePumpWithAsyncHandleMessage()
        {
            var runningTasks = new ConcurrentDictionary<Task, Task>();
            var semaphore = new SemaphoreSlim(100);
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var token = tokenSource.Token;
            int numberOfTasks = 0;

            var pumpTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await semaphore.WaitAsync(token).ConfigureAwait(false);
                    Interlocked.Increment(ref numberOfTasks);

                    var task = HandleMessageWithCancellation(token);

                    runningTasks.TryAdd(task, task);

                    task.ContinueWith(t =>
                    {
                        semaphore.Release();
                        Task taskToBeRemoved;
                        runningTasks.TryRemove(t, out taskToBeRemoved);
                    }, TaskContinuationOptions.ExecuteSynchronously)
                        .Ignore();
                }
            });

            await pumpTask.IgnoreCancellation().ConfigureAwait(false);
            await Task.WhenAll(runningTasks.Values).IgnoreCancellation().ConfigureAwait(false);
            tokenSource.Dispose();
            semaphore.Dispose();

            $"Consumed {numberOfTasks} messages with concurrency {semaphore.CurrentCount} in 5 seconds. Throughput {numberOfTasks / 5} msgs/s"
                .Output();
        }

        [Test]
        public async Task TheCompletePumpWithBlockingHandleMessage()
        {
            var runningTasks = new ConcurrentDictionary<Task, Task>();
            var semaphore = new SemaphoreSlim(100);
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            var token = tokenSource.Token;
            int numberOfTasks = 0;

            var pumpTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await semaphore.WaitAsync(token).ConfigureAwait(false);

                    var runningTask = Task.Run(() =>
                    {
                        Interlocked.Increment(ref numberOfTasks);

                        return BlockingHandleMessage();
                    }, CancellationToken.None);

                    runningTasks.TryAdd(runningTask, runningTask);

                    runningTask.ContinueWith(t =>
                    {
                        semaphore.Release();

                        Task taskToBeRemoved;
                        runningTasks.TryRemove(t, out taskToBeRemoved);
                    }, TaskContinuationOptions.ExecuteSynchronously)
                        .Ignore();
                }
            });

            await pumpTask.IgnoreCancellation().ConfigureAwait(false);
            await Task.WhenAll(runningTasks.Values).IgnoreCancellation().ConfigureAwait(false);
            tokenSource.Dispose();

            $"Consumed {numberOfTasks} messages with concurrency {semaphore.CurrentCount} in 5 seconds. Throughput {numberOfTasks / 5} msgs/s"
                .Output();
        }

        private static Task BlockingHandleMessage(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromCanceled(cancellationToken);
            }

            Thread.Sleep(1000);
            return Task.CompletedTask;
        }

        static object[] CreationOptionsAndConfigureAwait =
     {
            new object[] { TaskCreationOptions.None, false },
            new object[] { TaskCreationOptions.None, true },
            new object[] { TaskCreationOptions.HideScheduler, false },
            new object[] { TaskCreationOptions.HideScheduler, true },
        };

        [Test, TestCaseSource("CreationOptionsAndConfigureAwait")]
        public async Task LimitingThreads(TaskCreationOptions options, bool configureAwait)
        {
            $"Running with creation options {options} and ConfigureAwait({configureAwait})".Output();

            #region Cancellation AsBefore

            var runningTasks = new ConcurrentDictionary<Task, Task>();
            var semaphore = new SemaphoreSlim(800);
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            var token = tokenSource.Token;
            int numberOfTasks = 0;

            #endregion

            var scheduler = new LimitedConcurrencyLevelTaskScheduler(8);

            var pumpTask = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await semaphore.WaitAsync(token).ConfigureAwait(false);

                    var runningTask = Task.Factory.StartNew(() =>
                    {
                        Interlocked.Increment(ref numberOfTasks);

                        return HandleMessageUnderSpecialScheduler(configureAwait, token);
                    }, CancellationToken.None, options, scheduler)
                    .Unwrap();

                    #region Tracking and Housekeeping AsBefore

                    runningTasks.TryAdd(runningTask, runningTask);

                    runningTask.ContinueWith(t =>
                    {
                        semaphore.Release();

                        Task taskToBeRemoved;
                        runningTasks.TryRemove(t, out taskToBeRemoved);
                    }, TaskContinuationOptions.ExecuteSynchronously)
                        .Ignore();

                    #endregion
                }
            });

            await pumpTask.IgnoreCancellation().ConfigureAwait(false);
            await Task.WhenAll(runningTasks.Values).IgnoreCancellation().ConfigureAwait(false);
            tokenSource.Dispose();

            $"Consumed {numberOfTasks} messages with concurrency {semaphore.CurrentCount} in 5 seconds. Throughput {numberOfTasks / 5} msgs/s".Output();
        }

        static Task HandleMessageUnderSpecialScheduler(bool configureAwait, CancellationToken cancellationToken = default(CancellationToken))
        {
            var startNewTask = Task.Factory.StartNew(async () =>
            {
                #region Output

                var scheduler = TaskScheduler.Current as LimitedConcurrencyLevelTaskScheduler;
                if (scheduler != null)
                {
                    "StartNew: Before Delay Limiting Scheduler".Output();
                }
                else
                {
                    "StartNew: Before Delay".Output();
                }

                #endregion

                await Task.Delay(1000, cancellationToken).ConfigureAwait(configureAwait);

                #region Output

                scheduler = TaskScheduler.Current as LimitedConcurrencyLevelTaskScheduler;
                if (scheduler != null)
                {
                    "StartNew: After Delay Limiting Scheduler".Output();
                }
                else
                {
                    "StartNew: After Delay".Output();
                }

                #endregion

            })
            .Unwrap();

            var runTask = Task.Run(async () =>
            {
                #region Output

                var scheduler = TaskScheduler.Current as LimitedConcurrencyLevelTaskScheduler;
                if (scheduler != null)
                {
                    "Run: Before Delay Limiting Scheduler".Output();
                }
                else
                {
                    "Run: Before Delay".Output();
                }

                #endregion

                await Task.Delay(1000, cancellationToken).ConfigureAwait(configureAwait);

                #region Output

                scheduler = TaskScheduler.Current as LimitedConcurrencyLevelTaskScheduler;
                if (scheduler != null)
                {
                    "Run: After Delay Limiting Scheduler".Output();
                }
                else
                {
                    "Run: After Delay".Output();
                }

                #endregion

            });

            return Task.WhenAll(startNewTask, runTask);
        }
    }
}