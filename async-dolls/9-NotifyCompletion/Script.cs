using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncDolls.NotifyCompletion
{
    [TestFixture] 
    public class Script
    {
        [Test]
        public async Task Now()
        {
            await NowOrNever.Now;
        }

        [Test]
        public async Task Never()
        {
            await NowOrNever.Never;
        }

        // https://blog.scooletz.com/2017/06/12/await-now-or-never/
        public class NowOrNever : ICriticalNotifyCompletion
        {
            public static readonly NowOrNever Now = new NowOrNever(true);
            public static readonly NowOrNever Never = new NowOrNever(false);

            NowOrNever(bool isCompleted)
            {
                IsCompleted = isCompleted;
            }

            public NowOrNever GetAwaiter()
            {
                return this;
            }

            public void GetResult() { }

            public bool IsCompleted { get; }

            public void OnCompleted(Action continuation)
            {
            }

            public void UnsafeOnCompleted(Action continuation)
            {
            }
        }

        [Test]
        public async Task CultureAwait()
        {
            Console.WriteLine(CultureInfo.CurrentCulture);
            await CultureInfo.CurrentCulture;
            Console.WriteLine(CultureInfo.CurrentCulture);
        }

    }

    public static class CultureAwaitExtensions
    {
        public static CultureAwaiter GetAwaiter(this CultureInfo info)
        {
            return new CultureAwaiter(info);
        }

        public struct CultureAwaiter : ICriticalNotifyCompletion
        {
            private readonly CultureInfo cultureInfo;
            private Task task;

            public CultureAwaiter(CultureInfo cultureInfo)
            {
                this.cultureInfo = cultureInfo;
                task = Task.Delay(2000);
            }

            public bool IsCompleted => task.IsCompleted;

            public void OnCompleted(Action continuation)
            {
                task.GetAwaiter().OnCompleted(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                task.GetAwaiter().UnsafeOnCompleted(continuation);
            }

            public void GetResult()
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-us");
            }
        }
    }
}