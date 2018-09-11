using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using static System.Console;

namespace dishwasher
{
    [TestFixture]
    public class AsyncManualDishwasherUnloading
    {
        [Test]
        public async Task Unload()
        {
        }

        public static void Son(Action next)
        {
            WriteLine("Son");
            next();
        }

        public static void Wife(Action next)
        {
            WriteLine("Wife");
            next();
        }

        public static void Husband(Action next)
        {
            WriteLine("Husband");
            next();
        }

        public static void Done(Action next)
        {
            WriteLine("Dishwasher unloaded");
            next();
        }   
    }
}
