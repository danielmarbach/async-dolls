using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncDolls
{
    [TestFixture]
    public class AsyncDollsScript
    {
        [Test]
        public void TheDolls()
        {
            Action action = () => { Console.WriteLine("Done"); };

            Method1(() => Method2(() => Method3(action)));
        }

        [Test]
        public void TheDollsGeneric()
        {
            Action done = () => { Console.WriteLine("Done"); };

            var actions = new List<Action<Action>>
            {
                Method1,
                Method2,
                Method3,
                a => done()
            };

            Invoke(actions);
        }

        static void Method1(Action next)
        {
            Console.WriteLine("Method1");
            next();
        }

        static void Method2(Action next)
        {
            Console.WriteLine("Method2");
            next();
        }


        static void Method3(Action next)
        {
            Console.WriteLine("Method3");
            next();
        }

        static void Invoke(List<Action<Action>> actions, int currentIndex = 0)
        {
            if (currentIndex == actions.Count)
                return;

            var action = actions[currentIndex];
            action(() => Invoke(actions, currentIndex + 1));
        }

        [Test]
        public async Task TheAsyncDolls()
        {
            Func<Task> done = () =>
            {
                Console.WriteLine("Done");
                return Task.CompletedTask;
            };

            await MethodAsync1(() => MethodAsync2(() => MethodAsync3(done)));
        }

        [Test]
        public async Task TheAsyncDollsGeneric()
        {
            Func<Task> done = () =>
            {
                Console.WriteLine("Done");
                return Task.CompletedTask;
            };

            var actions = new List<Func<Func<Task>, Task>>
            {
                MethodAsync1,
                MethodAsync2,
                MethodAsync3,
                a => done()
            };

            await Invoke(actions);
        }

        static Task MethodAsync1(Func<Task> next)
        {
            Console.WriteLine("Method1");
            return next();
        }

        static Task MethodAsync2(Func<Task> next)
        {
            Console.WriteLine("Method2");
            return next();
        }

        static Task MethodAsync3(Func<Task> next)
        {
            Console.WriteLine("Method3");
            return next();
        }

        static Task Invoke(List<Func<Func<Task>, Task>> actions, int currentIndex = 0)
        {
            if (currentIndex == actions.Count)
                return Task.CompletedTask;

            var action = actions[currentIndex];
            return action(() => Invoke(actions, currentIndex + 1));
        }

        [Test]
        public async Task WhatThisAllowsUsTodo()
        {
            Func<Func<Task>, Task> exceptionHandler = async next =>
            {
                try
                {
                    await next();
                }
                catch (Exception)
                {
                    Console.WriteLine("We caught an exception for you");
                }
            };

            var actions = new List<Func<Func<Task>, Task>>
            {
                MethodAsync1,
                MethodAsync2,
                MethodAsync3,
                exceptionHandler,
                EvilMethod
            };

            await Invoke(actions);
        }

        static Task EvilMethod(Func<Task> next)
        {
            Console.WriteLine("Entering EvilMethod");
            throw new InvalidOperationException();
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
            
            var invoker = new PipelineInvoker(actions);

            await invoker.Invoke(context);
        }

        static async Task MethodWhichExecutesRestMultipleTimes(Context context, Func<Task> next)
        {
            context.GetLogger()("MethodWhichExecutesRestMultipleTimes");
            for (int i = 0; i < 3; i++)
            {
                await next().ConfigureAwait(false);
            }
        }

        class PipelineInvoker
        {
            private readonly List<Func<Context, Func<Task>, Task>> executingActions;

            public PipelineInvoker(IEnumerable<Func<Context, Func<Task>, Task>> actions)
            {
                executingActions = new List<Func<Context, Func<Task>, Task>>(actions);
            }

            public Task Invoke(Context context, int currentIndex = 0)
            {
                if (currentIndex == executingActions.Count)
                    return Task.CompletedTask;

                var action = executingActions[currentIndex];
                return action(context, () => Invoke(context, currentIndex + 1));
            }
        }
    }

    class Context
    {
        readonly Dictionary<string, object> stash = new Dictionary<string, object>();

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