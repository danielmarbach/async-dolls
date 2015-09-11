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
                return Task.FromResult(0);
            };

            await MethodAsync1(() => MethodAsync2(() => MethodAsync3(done)));
        }

        [Test]
        public async Task TheAsyncDollsGeneric()
        {
            Func<Task> done = () =>
            {
                Console.WriteLine("Done");
                return Task.FromResult(0);
            };

            var actions = new Queue<Func<Func<Task>, Task>>();

            actions.Enqueue(MethodAsync1);
            actions.Enqueue(MethodAsync2);
            actions.Enqueue(MethodAsync3);

            // Partial application
            actions.Enqueue(a => done());

            await Invoke(actions);
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

        static Task EvilMethod(Func<Task> next)
        {
            Console.WriteLine("Entering EvilMethod");
            throw new InvalidOperationException();
        }

        static Task Invoke(Queue<Func<Func<Task>, Task>> actions)
        {
            if (actions.Count == 0)
                return Task.FromResult(0);

            var action = actions.Dequeue();
            return action(() => Invoke(actions));
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
                return Task.FromResult(0);

            var action = actions.Dequeue();
            return action(context, () => Invoke(context, actions));
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