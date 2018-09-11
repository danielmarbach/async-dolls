using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace dishwasher
{
    [TestFixture]
    public class AsyncAutomaticDishwasherUnloading
    {
        [Test]
        public async Task Unload()
        {
        }

        static void Invoke(List<Action<Action>> actions, int currentIndex = 0)
        {
            if (currentIndex == actions.Count)
            {
                return;
            }

            var action = actions[currentIndex];
            action(() => Invoke(actions, ++currentIndex));
        }

        public static void Son(Action next)
        {
            Console.WriteLine("Son");
            next();
        }

        public static void Wife(Action next)
        {
            Console.WriteLine("Wife");
            next();
        }

        public static void Husband(Action next)
        {
            Console.WriteLine("Husband");
            next();
        }

        public static void Done(Action next)
        {
            Console.WriteLine("Dishwasher unloaded");
        }
    }
}