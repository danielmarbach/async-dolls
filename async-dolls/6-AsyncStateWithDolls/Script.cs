using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncDolls
{
    [TestFixture]
    public class AsyncStateWithDollsScript
    {
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

        [Test]
        public async Task TheAsyncDollWithFloatingState()
        {
            var actions = new List<Func<Context, Func<Task>, Task>>
            {
                MethodAsyncWithContext1,
                MethodAsyncWithContext2,
                MethodAsynWithContextc3
            };

            var context = new Context();
            context.SetLogger(Console.WriteLine);

            await Invoke(context, actions);
        }

        static Task MethodAsyncWithContext1(Context context, Func<Task> next)
        {
            context.GetLogger()("Method1");
            return next();
        }

        static Task MethodAsyncWithContext2(Context context, Func<Task> next)
        {
            context.GetLogger()("Method2");
            return next();
        }

        static Task MethodAsynWithContextc3(Context context, Func<Task> next)
        {
            context.GetLogger()("Method3");
            return next();
        }

        static Task Invoke(Context context, List<Func<Context, Func<Task>, Task>> actions, int currentIndex = 0)
        {
            if (currentIndex == actions.Count)
                return Task.CompletedTask;

            var action = actions[currentIndex];
            return action(context, () => Invoke(context, actions, currentIndex + 1));
        }

        [Test]
        public async Task NowItGetsALittleBitCrazy()
        {
            var actions = new List<Func<Context, Func<Task>, Task>>
            {
                MethodAsyncWithContext1,
                MethodWhichExecutesRestMultipleTimes,
                MethodAsyncWithContext2,
                MethodAsynWithContextc3
            };

            var context = new Context();
            context.SetLogger(Console.WriteLine);
            
            await Invoke(context, actions);
        }

        static async Task MethodWhichExecutesRestMultipleTimes(Context context, Func<Task> next)
        {
            context.GetLogger()("MethodWhichExecutesRestMultipleTimes");
            for (int i = 0; i < 3; i++)
            {
                await next().ConfigureAwait(false);
            }
        }
    }

    public class Context
    {
        readonly Dictionary<string, object> stash = new Dictionary<string, object>();

        public Context()
        {
        }

        public Context(Context parent)
        {
            foreach (KeyValuePair<string, object> pair in parent.stash)
            {
                stash.Add(pair.Key, pair.Value);
            }
        }

        public void Set<T>(T value)
        {
            stash.Add(typeof(T).FullName, value);
        }

        public T Get<T>()
        {
            return (T)stash[typeof(T).FullName];
        }
    }

    static class ContextExtensions
    {
        public static void SetLogger(this Context context, Action<string> logger)
        {
            context.Set(new Logger { Debug = logger });
        }

        public static Action<string> GetLogger(this Context context)
        {
            var logger = context.Get<Logger>();
            return logger.Debug;
        }

        class Logger
        {
            public Action<string> Debug { get; internal set; } 
        }
    }
}