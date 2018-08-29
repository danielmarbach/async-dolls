using System;
using NUnit.Framework;
using static System.Console;

namespace dishwasher
{
    [TestFixture]
    public class ManualDishwasherUnloading
    {
        [Test]
        public void Unload()
        {
        }

        public static void Son()
        {
            WriteLine("Son");
        }

        public static void Wife()
        {
            WriteLine("Wife");
        }

        public static void Husband()
        {
            WriteLine("Husband");
        }

        public static void Done()
        {
            WriteLine("Dishwasher unloaded");
        }   
    }
}
