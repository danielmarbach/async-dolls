using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using NUnit.Framework;

namespace AsyncDolls
{
    [TestFixture]
    public class AsyncTplScript
    {
        [Test]
        public async Task Unwrapping()
        {
            #region Output

            "Starting proxy task".Output();

            #endregion

            Task<Task> proxyTask = Task.Factory.StartNew(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                #region Output

                "Done inside proxy".Output();

                #endregion
            });
            await proxyTask;

            #region Output

            "Proxy task done.".Output();

            "Starting actual task".Output();

            #endregion

            Task actualTask = Task.Factory.StartNew(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));

                #region Output

                "Done inside actual".Output();

                #endregion

            }).Unwrap();

            await actualTask;

            #region Output

            "Actual task done.".Output();

            #endregion

        }

        [Test]
        public async Task CancellingTheTask()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.Cancel();
            var token = tokenSource.Token;
            
            var cancelledTask = Task.Run(() => { }, token);

            #region Output

            cancelledTask.Status.ToString().Output();

            #endregion
            try
            {
                await cancelledTask;
            }
            catch (OperationCanceledException)
            {
                #region Output

                "Throws when awaited".Output();
                cancelledTask.Status.ToString().Output();

                #endregion

            }
        }

        [Test]
        public async Task CancelllingTheOperationInsideTheTask()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(5));
            var token = tokenSource.Token;

            var cancelledTask = Task.Run(async () => { await Task.Delay(TimeSpan.FromMinutes(1), token); }, token); // Passing in the token only means the task is transitioning into cancelled state
           
            #region Output

            cancelledTask.Status.ToString().Output();

            #endregion

            try
            {
                await cancelledTask;
            }
            catch (OperationCanceledException)
            {
                #region Output

                "Throws when awaited".Output();
                cancelledTask.Status.ToString().Output();

                #endregion
            }
        }

        [Test]
        public async Task GracefulShutdown()
        {
            var tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(TimeSpan.FromSeconds(5));
            var token = tokenSource.Token;

            var cancelledTask = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(1), token).IgnoreCancellation();
            });

            #region Output

            cancelledTask.Status.ToString().Output();

            #endregion

            try
            {
                await cancelledTask;
            }
            catch (OperationCanceledException)
            {
                #region Output

                "Should not throw when awaited".Output();
                cancelledTask.Status.ToString().Output();

                #endregion
            }

            #region Output

            "Done".Output();

            #endregion

        }

        [Test]
        public async Task AsyncRecursionWithExceptionHandling()
        {
            var sender = new Sender();
            await sender.RetryOnThrottle(s => s.SendAsync(), TimeSpan.FromMilliseconds(10), 1);
        }
    }

    public interface IMessageSender
    {
        Task SendAsync();
    }

    internal class Sender : IMessageSender
    {
        private int numberOfTimes = 0;

        public async Task SendAsync()
        {
            if (numberOfTimes++ <= 3)
            {
                await Task.Delay(1000);
                throw new InvalidOperationException();
            }
        }
    }

    static class MessageSenderExtensions
    {
        public static Task RetryOnThrottle(this IMessageSender sender, Func<IMessageSender, Task> action, TimeSpan delay, int maxRetryAttempts, int retryAttempts = 0)
        {
            var task = action(sender);

            return task.ContinueWith(async t =>
            {
                var exception = ExceptionDispatchInfo.Capture(t.Exception?.InnerException);
                var serverBusy = exception.SourceException is InvalidOperationException;

                if (serverBusy && retryAttempts < maxRetryAttempts)
                {
                    await Task.Delay(delay);
                    await sender.RetryOnThrottle(action, delay, maxRetryAttempts, ++retryAttempts);
                }
                else if(t.IsFaulted)
                {
                    exception.Throw();
                }
            })
            .Unwrap();
        }
    }
}