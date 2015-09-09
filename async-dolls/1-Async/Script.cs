using System;
using System.IO;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace AsyncDolls
{
    /// <summary>
    /// Contains a lot of white space. Optimized for Consolas 14 pt 
    /// and Full HD resolution
    /// </summary>
    [TestFixture]
    public class AsyncScript
    {
        [SetUp]
        public void SetUp()
        {
        }




        [Test]
        public void ThatsMe()
        {
            var daniel = new DanielMarbach();
            daniel
                .Is("CEO").Of("tracelight Gmbh").In("Switzerland")
                .and
                .WorkingFor("Particular Software").TheFolksBehind("NServiceBus")
                .Reach("@danielmarbach")
                .Reach("www.planetgeek.ch");
        }

        [Test]
        public async Task AsyncRecap()
        {
            var slide = new Slide(title: "Asynchronous vs. Parallel");
            await slide
                .Sample(async () =>
                {
                    // Parallel
                    Parallel.For(0, 1000, CpuBoundMethod); // or Parallel.ForEach
                    await Task.Run(() => CpuBoundMethod(10)); // or Task.Factory.StartNew(), if in doubt use Task.Run

                    // Asynchronous
                    await IoBoundMethod(".\\IoBoundMethod.txt"); // if true IOBound don't use Task.Run, StartNew
                });
        }

        static void CpuBoundMethod(int i)
        {
            Console.WriteLine(i);
        }

        static async Task IoBoundMethod(string path)
        {
            using (var stream = new FileStream(path, FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(stream))
            {
                await writer.WriteLineAsync("Yehaa " + DateTime.Now);
                await writer.FlushAsync();
                writer.Close();
                stream.Close();
            }
        }
    }
}