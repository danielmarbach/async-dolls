using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
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
            var slide = new Slide(title: "Asynchronous vs. Parallel");
            await slide
                .Sample(async () =>
                {
                    // Parallel
                    Parallel.For(0, 1000, CpuBoundMethod); // or Parallel.ForEach
                    await Task.Run(() => CpuBoundMethod(10)); // or Task.Factory.StartNew(), if in doubt use Task.Run

                    // Asynchronous
                    await IoBoundMethod(".\\IoBoundMethod.txt"); // if true IOBound don't use Task.Run, StartNew
                });
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
            var slide = new Slide(title: "Best Practices: async Task over async void");
            await slide
                .Sample(async () =>
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
                });
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
            var slide = new Slide(title: "Best Practices: ConfigureAwait(false)");
            await slide
                .Sample(async () =>
                {
                    SynchronizationContext.SetSynchronizationContext(new Syncy("The context"));

                    Console.WriteLine(SynchronizationContext.Current == null ? "Before IoBoundMethod2 without ConfigureAwait(false)" : SynchronizationContext.Current.ToString());

                    await IoBoundMethod2(".\\IoBoundMethod2.txt");

                    Console.WriteLine(SynchronizationContext.Current == null ? "Before IoBoundMethod2 with ConfigureAwait(false)" : SynchronizationContext.Current.ToString());

                    await IoBoundMethod2(".\\IoBoundMethod2.txt").ConfigureAwait(false);

                    Console.WriteLine(SynchronizationContext.Current == null ? "After IoBoundMethod2 with ConfigureAwait(false)" : SynchronizationContext.Current.ToString());
                });
        }

        static async Task IoBoundMethod2(string path)
        {
            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(stream))
            {
                Console.WriteLine(SynchronizationContext.Current == null ? "IoBoundMethod2" : SynchronizationContext.Current.ToString());
                await writer.WriteLineAsync("Yehaa " + DateTime.Now);
                Console.WriteLine(SynchronizationContext.Current == null ? "IoBoundMethod2" : SynchronizationContext.Current.ToString());
                await writer.FlushAsync();
                writer.Close();
                stream.Close();
            }
        }

        [Test]
        public async Task DontMixBlockingAndAsync()
        {
            var slide = new Slide(title: "Best Practices: Don't mix blocking code with async. Async all the way!");
            await slide
                .Sample(() =>
                {
                    SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext()); // Let's simulate wpf stuff

                    Delay(15); // what happens here? How can we fix this?
                });
        }

        static void Delay(int milliseconds)
        {
            DelayAsync(milliseconds).Wait(); // Similar evilness is Thread.Sleep, Semaphore.Wait..
        }

        static Task DelayAsync(int milliseconds)
        {
            return Task.Delay(milliseconds);
        }

        [Test]
        public async Task ACompleteExampleMixingAsynchronousAndParallelProcessing()
        {
            var output = new ConcurrentQueue<string>();
            var semaphore = new SemaphoreSlim(2);
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var token = tokenSource.Token;
            var scheduler = new QueuedTaskScheduler(TaskScheduler.Default, 2);

            try
            {
                await Task.Factory.StartNew(async () =>
                {
                    await Process(token, semaphore, output, scheduler);

                }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .Unwrap();
            }
            catch (OperationCanceledException)
            {
            }

            PrintOUt(output);
        }

        private static void PrintOUt(ConcurrentQueue<string> output)
        {
            foreach (var o in output)
            {
                Console.WriteLine(o);
            }
        }

        private static async Task Process(CancellationToken token, SemaphoreSlim semaphore, ConcurrentQueue<string> output,
            QueuedTaskScheduler scheduler)
        {
            while (!token.IsCancellationRequested)
            {
                await semaphore.WaitAsync(token);

                InnerProcess(token, semaphore, output, scheduler);
            }
        }

        private static void InnerProcess(CancellationToken token, SemaphoreSlim semaphore,
            ConcurrentQueue<string> output,
            QueuedTaskScheduler scheduler)
        {
            int taskNumber = 0;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Factory.StartNew(async () =>
            {
                int nr = Interlocked.Increment(ref taskNumber);

                output.Enqueue("Kick off " + nr + " " + Thread.CurrentThread.ManagedThreadId);
                await Task.Delay(5000).ConfigureAwait(false);
                output.Enqueue(" back " + nr + " " + Thread.CurrentThread.ManagedThreadId);

                semaphore.Release();
            }, token, TaskCreationOptions.AttachedToParent | TaskCreationOptions.HideScheduler, scheduler)
                .Unwrap();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }
    }
}