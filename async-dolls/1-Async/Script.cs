using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Threading;
using NUnit.Framework;

namespace AsyncDolls
{
    /// <summary>
    /// Contains a lot of white space. Optimized for Consolas 14 pt 
    /// and Full HD resolution
    /// </summary>
    [TestFixture]
    public class AsyncScript
    {
        [Test]
        public void ThatsMe()
        {
            var daniel = new DanielMarbach();
            daniel
                .Is("CEO").Of("tracelight Gmbh").In("Switzerland")
                .and
                .WorkingFor("Particular Software").TheFolksBehind("NServiceBus")
                .Reach("@danielmarbach")
                .Reach("www.planetgeek.ch");
        }

        [Test]
        public async Task AsyncRecap()
        {
            // Parallel
            Parallel.For(0, 1000, CpuBoundMethod); // or Parallel.ForEach
            await Task.Run(() => CpuBoundMethod(10)); // or Task.Factory.StartNew(), if in doubt use Task.Run

            // Asynchronous
            await IoBoundMethod(".\\IoBoundMethod.txt"); // if true IOBound don't use Task.Run, StartNew
        }

        static void CpuBoundMethod(int i)
        {
            Console.WriteLine(i);
        }

        static async Task IoBoundMethod(string path)
        {
            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync("Yehaa " + DateTime.Now);
                await writer.FlushAsync();
                writer.Close();
                stream.Close();
            }
        }

        [Test]
        public async Task AsyncVoid()
        {
            try
            {
                AvoidAsyncVoid();
            }
            catch (InvalidOperationException e)
            {
                // where is the exception?
                Console.WriteLine(e);
            }
            await Task.Delay(100);
        }

        static async void AvoidAsyncVoid() // Fire & Forget, can't be awaited, exception: EventHandlers
        {
            Console.WriteLine("Going inside async void.");
            await Task.Delay(10);
            Console.WriteLine("Going to throw soon");
            throw new InvalidOperationException("Gotcha!");
        }

        [Test]
        public async Task ConfigureAwait()
        {
            // Attention: In unit test everything behaves differently, I'll explain why
            // ReSharper disable once PossibleNullReferenceException
            await Process.Start(new ProcessStartInfo(@".\configureawait.exe") { UseShellExecute = false });
        }

        [Test]
        public void DontMixBlockingAndAsync()
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext()); // Let's simulate wpf stuff

            Delay(15); // what happens here? How can we fix this?
        }

        static void Delay(int milliseconds)
        {
            DelayAsync(milliseconds).Wait(); // Similar evilness is Thread.Sleep, Semaphore.Wait..
        }

        static async Task DelayAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }

        [Test]
        public async Task ACompleteExampleMixingConcurrentAndAsynchronousProcessingWithPotentialBlockingOperations()
        {
            var runningTasks = new ConcurrentDictionary<Task, Task>();
            var semaphore = new SemaphoreSlim(100);
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var token = tokenSource.Token;
            // var scheduler = new QueuedTaskScheduler(TaskScheduler.Default, 2);
            var scheduler = TaskScheduler.Default;

            try
            {
                var pumpTask = Task.Factory.StartNew(async () =>
                {
                    int taskNumber = 0;
                    while (!token.IsCancellationRequested)
                    {
                        await semaphore.WaitAsync(token);

                        var task = Task.Factory.StartNew(async () =>
                        {
                            int nr = Interlocked.Increment(ref taskNumber);

                            Console.WriteLine("Kick off " + nr + " " + Thread.CurrentThread.ManagedThreadId);
                            await LibraryCallWhichIsNotTrulyAsync().ConfigureAwait(false);
                            Console.WriteLine(" back " + nr + " " + Thread.CurrentThread.ManagedThreadId);

                            semaphore.Release();
                        }, CancellationToken.None, TaskCreationOptions.HideScheduler, scheduler)
                        .Unwrap();

                        task.ContinueWith(t =>
                        {
                            Task taskToBeRemoved;
                            runningTasks.TryRemove(t, out taskToBeRemoved);
                        }, TaskContinuationOptions.ExecuteSynchronously)
                        .Ignore();

                        runningTasks.AddOrUpdate(task, task, (k, v) => task)
                        .Ignore();
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .Unwrap();

                await pumpTask;
            }
            catch (OperationCanceledException)
            {
            }

            await Task.WhenAll(runningTasks.Values);
        }

        // For example MSMQ has no true async API especially when you are dealing with transactions. Then you need to 
        // use .Receive which is a blocking operation. 
        private static Task LibraryCallWhichIsNotTrulyAsync()
        {
            Thread.Sleep(1000);
            return Task.FromResult(0);
        }

        [Test]
        public async Task ACompleteExampleMixingConcurrentAndAsynchronousProcessingWithTrueAsyncOperations()
        {
            var runningTasks = new ConcurrentDictionary<Task, Task>();
            var semaphore = new SemaphoreSlim(100);
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var token = tokenSource.Token;

            try
            {
                var pumpTask = Task.Factory.StartNew(async () =>
                {
                    int taskNumber = 0;
                    while (!token.IsCancellationRequested)
                    {
                        await semaphore.WaitAsync(token);
                        int nr = Interlocked.Increment(ref taskNumber);

                        Console.WriteLine("Kick off " + nr + " " + Thread.CurrentThread.ManagedThreadId);
                        var task = LibraryCallWhichIsTrulyAsync();

                        task.ContinueWith(t =>
                        {
                            Console.WriteLine(" back " + nr + " " + Thread.CurrentThread.ManagedThreadId);
                            semaphore.Release();
                            Task taskToBeRemoved;
                            runningTasks.TryRemove(t, out taskToBeRemoved);
                        }, TaskContinuationOptions.ExecuteSynchronously)
                        .Ignore();

                        runningTasks.AddOrUpdate(task, task, (k, v) => task)
                        .Ignore();
                    }
                }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .Unwrap();

                await pumpTask;
            }
            catch (OperationCanceledException)
            {
            }
            await Task.WhenAll(runningTasks.Values);
        }

        private static Task LibraryCallWhichIsTrulyAsync()
        {
            return Task.Delay(1000);
        }

        static ThreadLocal<int> ThreadLocal = new ThreadLocal<int>(() => 0);

        static AsyncLocal<int> AsyncLocal = new AsyncLocal<int>();

        static dynamic Local;

        [Test]
        public async Task AsyncLocalForTheWin()
        {
            Local = AsyncLocal;

            Console.WriteLine($"Before TopOne: '{Local.Value}'");
            await TopOne().ConfigureAwait(false);
            Console.WriteLine($"After TopOne: '{Local.Value}'");
            await TopTen().ConfigureAwait(false);
            Console.WriteLine($"After TopTen: '{Local.Value}'");
        }

        [Test]
        public async Task ThreadLocalNey()
        {
            Local = ThreadLocal;

            Console.WriteLine($"Before TopOne: '{Local.Value}'");
            await TopOne().ConfigureAwait(false);
            Console.WriteLine($"After TopOne: '{Local.Value}'");
            await TopTen().ConfigureAwait(false);
            Console.WriteLine($"After TopTen: '{Local.Value}'");
        }

        static async Task TopOne()
        {
            await Task.Delay(10).ConfigureAwait(false);
            Local.Value = 1;
            await SomewhereElse().ConfigureAwait(false);
        }

        static async Task TopTen()
        {
            await Task.Delay(10).ConfigureAwait(false);
            Local.Value = 10;
            await SomewhereElse().ConfigureAwait(false);
        }

        static async Task SomewhereElse()
        {
            await Task.Delay(10).ConfigureAwait(false);
            Console.WriteLine($"Inside Somewhere: '{Local.Value}'");
            await Task.Delay(10).ConfigureAwait(false);
            await DeepDown();
        }

        static async Task DeepDown()
        {
            await Task.Delay(10).ConfigureAwait(false);
            Console.WriteLine($"Inside DeepDown: '{Local.Value}'");
            Fire().Ignore();
        }

        static async Task Fire()
        {
            await Task.Yield();
            Console.WriteLine($"Inside Fire: '{Local.Value}'");
        }
    }

    static class TaskExtensions
    {
        public static void Ignore(this Task task)
        {
        }
    }

    public static class ProcessExtensions
    {
        public static TaskAwaiter<int> GetAwaiter(this Process process)
        {
            var tcs = new TaskCompletionSource<int>();
            process.EnableRaisingEvents = true;
            process.Exited += (s, e) => tcs.TrySetResult(process.ExitCode);
            if (process.HasExited) tcs.TrySetResult(process.ExitCode);
            return tcs.Task.GetAwaiter();
        }
    }
}