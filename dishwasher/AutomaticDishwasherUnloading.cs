﻿using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace dishwasher
{
    [TestFixture]
    public class AutomaticDishwasherUnloading
    {
        [Test]
        public void Unload()
        {
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