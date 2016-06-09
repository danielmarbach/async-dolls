using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
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
                    Console.WriteLine("We caught an exception for you.");
                }
            };

            var actions = new List<Func<Func<Task>, Task>>
            {
                Retrier,
                MethodAsync1,
                MethodAsync2,
                MethodAsync3,
                //exceptionHandler,
                EvilMethod
            };

            await Invoke(actions);
        }

        static Task EvilMethod(Func<Task> next)
        {
            Console.WriteLine("Entering EvilMethod");
            throw new InvalidOperationException();
        }

        static async Task Retrier(Func<Task> next)
        {
            ExceptionDispatchInfo exceptionDispatchInfo = null;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await next();
                    exceptionDispatchInfo = null;
                    break;
                }
                catch (Exception ex)
                {
                    exceptionDispatchInfo = ExceptionDispatchInfo.Capture(ex);
                }
            }

            exceptionDispatchInfo?.Throw();
        }
    }
}