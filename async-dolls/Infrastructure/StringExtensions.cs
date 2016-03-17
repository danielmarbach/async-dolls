using System;

namespace AsyncDolls
{
    static class StringExtensions
    {
        public static void Output(this string value)
        {
            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss:fff") + ": " + value);
        }
    }
}