using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncDolls.ValueTaskScript
{
    [TestFixture]
    public class Script
    {

        [Test]
        public async Task Test()
        {
            var cache = new Cache();
            var value1 = await cache.Get("Foo");
            var value2 = await cache.Get("Foo");

            Assert.AreEqual(42, value1);
            Assert.AreEqual(value1, value2);
        }

        public class Cache
        {
            ConcurrentDictionary<string, int> cachedValues = new ConcurrentDictionary<string, int>();

            public async ValueTask<int> Get(string key)
            {
                if (cachedValues.TryGetValue(key, out int value))
                {
                    return value;
                }

                using (var stream = File.OpenText(@"8-ValueTask\values.txt"))
                {
                    string line;
                    while ((line = await stream.ReadLineAsync().ConfigureAwait(false)) != null)
                    {
                        var splitted = line.Split(Convert.ToChar(";"));
                        var k = splitted[0];
                        var v = Convert.ToInt32(splitted[1]);

                        if (k != key)
                        {
                            continue;
                        }

                        cachedValues.TryAdd(k, v);
                        return v;
                    }
                }
                return 0;
            }
        }
    }
}