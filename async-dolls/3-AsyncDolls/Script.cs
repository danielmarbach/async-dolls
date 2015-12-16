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
            var actions = new Queue<Action<Action>>();

            actions.Enqueue(Method1);
            actions.Enqueue(Method2);
            actions.Enqueue(Method3);

            // Partial application
            actions.Enqueue(a => done());

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

        static void Invoke(Queue<Action<Action>> actions)
        {
            if(actions.Count == 0)
                return;

            var action = actions.Dequeue();
            action(() => Invoke(actions));
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

            var actions = new Queue<Func<Func<Task>, Task>>();

            actions.Enqueue(MethodAsync1);
            actions.Enqueue(MethodAsync2);
            actions.Enqueue(MethodAsync3);

            // Partial application
            actions.Enqueue(a => done());

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

        static Task Invoke(Queue<Func<Func<Task>, Task>> actions)
        {
            if (actions.Count == 0)
                return Task.CompletedTask;

            var action = actions.Dequeue();
            return action(() => Invoke(actions));
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

            var actions = new Queue<Func<Func<Task>, Task>>();

            actions.Enqueue(MethodAsync1);
            actions.Enqueue(MethodAsync2);
            actions.Enqueue(MethodAsync3);
            actions.Enqueue(exceptionHandler);
            actions.Enqueue(EvilMethod);

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
            var actions = new Queue<Func<Context, Func<Task>, Task>>();

            actions.Enqueue(MethodAsyncWithContext1);
            actions.Enqueue(MethodAsyncWithContext2);
            actions.Enqueue(MethodAsynWithContextc3);

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

        static Task Invoke(Context context, Queue<Func<Context, Func<Task>, Task>> actions)
        {
            if (actions.Count == 0)
                return Task.CompletedTask;

            var action = actions.Dequeue();
            return action(context, () => Invoke(context, actions));
        }

        [Test]
        public async Task NowItGetsALittleBitCrazy()
        {
            var actions = new Queue<Func<Context, Func<Task>, Task>>();

            actions.Enqueue(MethodAsyncWithContext1);
            actions.Enqueue(MethodWhichExecutesRestMultipleTimes);
            actions.Enqueue(MethodAsyncWithContext2);
            actions.Enqueue(MethodAsynWithContextc3);

            var context = new Context();
            context.SetLogger(Console.WriteLine);
            
            var invoker = new PipelineInvoker();
            invoker.Actions(actions);
            context.SetSnapshotter(invoker.TakeSnapshot, invoker.RestoreSnapshot);

            await invoker.Invoke(context);
        }

        static async Task MethodWhichExecutesRestMultipleTimes(Context context, Func<Task> next)
        {
            context.GetLogger()("MethodWhichExecutesRestMultipleTimes");
            for (int i = 0; i < 3; i++)
            {
                using (context.TakeSnapshot())
                {
                    await next().ConfigureAwait(false);
                }
            }
        }

        class PipelineInvoker
        {
            readonly Stack<Queue<Func<Context, Func<Task>, Task>>>  stack = new Stack<Queue<Func<Context, Func<Task>, Task>>>();
            Queue<Func<Context, Func<Task>, Task>>  executingActions = new Queue<Func<Context, Func<Task>, Task>>();

            public void Actions(IEnumerable<Func<Context, Func<Task>, Task>> actions)
            {
                executingActions = new Queue<Func<Context, Func<Task>, Task>>(actions);
            }

            public void TakeSnapshot()
            {
                stack.Push(new Queue<Func<Context, Func<Task>, Task>>(executingActions));
            }

            public void RestoreSnapshot()
            {
                // Slightly evil, but reference assignments are atomic
                executingActions = stack.Pop();
            }

            public Task Invoke(Context context)
            {
                if (executingActions.Count == 0)
                    return Task.CompletedTask;

                var action = executingActions.Dequeue();
                return action(context, () => Invoke(context));
            }
        }

        [Test]
        [Explicit]
        public async Task TheEnd()
        {
            var giveAway = new GiveAway();
            await giveAway.WorthThousandDollars();
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

    static class ContextSnapshotExtensions
    {
        public static void SetSnapshotter(this Context context, Action take, Action restore)
        {
            var snapshotter = new Snapshotter { Take = take, Restore = restore };
            context.Set(snapshotter);
        }

        public static IDisposable TakeSnapshot(this Context context)
        {
            return new SnapshotRegion(context);
        }

        class Snapshotter
        {
            public Action Take { get; internal set; }
            public Action Restore { get; internal set; }
        }

        class SnapshotRegion : IDisposable
        {
            private readonly Context context;

            public SnapshotRegion(Context context)
            {
                this.context = context;
                context.Get<Snapshotter>().Take();
            }

            public void Dispose()
            {
                context.Get<Snapshotter>().Restore();
            }
        }
    }
}