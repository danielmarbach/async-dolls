using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NUnit.Framework;

namespace AsyncDolls
{
    [TestFixture]
    public class AsyncScript
    {
        [Test]
        public void ThatsMe()
        {
            var daniel = new DanielMarbach();
            daniel
                .Is("CEO").Of("tracelight GmbH").In("Switzerland")
                .and
                .WorkingFor("Particular Software").TheFolksBehind("NServiceBus")
                .Reach("@danielmarbach")
                .Reach("www.planetgeek.ch");
        }

        [Test]
        public async Task CPUBound()
        {
            Parallel.For(0, 1000, CpuBoundMethod);
            Parallel.ForEach(Enumerable.Range(1000, 2000), CpuBoundMethod);

            await Task.Run(() => CpuBoundMethod(2001));
            await Task.Factory.StartNew(() => CpuBoundMethod(2002));
        }

        static void CpuBoundMethod(int i)
        {
            Console.WriteLine(i);
        }

        [Test]
        public async Task IOBound()
        {
            await IoBoundMethod();
        }

        static async Task IoBoundMethod()
        {
            using (var stream = new FileStream(".\\IoBoundMethod.txt", FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync("42");
                writer.Close();
                stream.Close();
            }
        }

        [Test]
        public async Task Sequential()
        {
            var sequential = Enumerable.Range(0, 4).Select(t => Task.Delay(1500));

            foreach (var task in sequential)
            {
                await task;
            }
        }

        [Test]
        public async Task Concurrent()
        {
            var concurrent = Enumerable.Range(0, 4).Select(t => Task.Delay(1500));
            await Task.WhenAll(concurrent);
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
                Console.WriteLine(e);
            }
            await Task.Delay(100);
        }

        static async void AvoidAsyncVoid()
        {
            Console.WriteLine("Going inside async void.");
            await Task.Delay(10);
            Console.WriteLine("Going to throw soon");
            throw new InvalidOperationException("Gotcha!");
        }

        [Test]
        public async Task ConfigureAwait()
        {
            // ReSharper disable once PossibleNullReferenceException
            await Process.Start(new ProcessStartInfo(@".\configureawait.exe") { UseShellExecute = false });
        }

        [Test]
        public void DontMixBlockingAndAsync()
        {
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());

            Delay(15);
        }


        static void Delay(int milliseconds)
        {
            DelayAsync(milliseconds).Wait();
        }

        static async Task DelayAsync(int milliseconds)
        {
            await Task.Delay(milliseconds);
        }

        [Test]
        public async Task ShortcutTheStatemachine()
        {
            await DoesNotShortcut();

            await DoesShortcut();
        }

        private static async Task DoesNotShortcut()
        {
            await Task.Delay(1);
        }

        private static Task DoesShortcut()
        {
            return Task.Delay(1);
        }

        /*
private static Task DoesNotShortcut()
{
  AsyncScript.\u003CDoesNotShortcut\u003Ed__12 stateMachine;
  stateMachine.\u003C\u003Et__builder = AsyncTaskMethodBuilder.Create();
  stateMachine.\u003C\u003E1__state = -1;
  stateMachine.\u003C\u003Et__builder.Start<AsyncScript.\u003CDoesNotShortcut\u003Ed__12>(ref stateMachine);
  return stateMachine.\u003C\u003Et__builder.Task;
}
private static Task DoesShortcut()
{
  return Task.Delay(1);
}

*/

        static ThreadLocal<string> ThreadLocal = new ThreadLocal<string>(() => "Initial Value");

        static AsyncLocal<string> AsyncLocal = new AsyncLocal<string> { Value = "Initial Value" };

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
            Local.Value = "ValueSetBy TopOne";
            await SomewhereElse().ConfigureAwait(false);
        }

        static async Task TopTen()
        {
            await Task.Delay(10).ConfigureAwait(false);
            Local.Value = "ValueSetBy TopTen";
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
}