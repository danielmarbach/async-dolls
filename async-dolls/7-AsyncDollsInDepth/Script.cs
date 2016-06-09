using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace AsyncDolls.AsyncDollsInDepth
{
    [TestFixture]
    public class Spec
    {
        [Test]
        public async Task Do()
        {
            var messages = new ConcurrentQueue<TransportMessage>();
            messages.Enqueue(new TransportMessage());
            messages.Enqueue(new TransportMessage());
            messages.Enqueue(new TransportMessage());

            var countdown = new AsyncCountdownEvent(3);

            var chainFactory = new ChainFactory();
            chainFactory.Register(() => new LogElement(countdown));
            chainFactory.Register(() => new DelayBefore());
            chainFactory.Register(() => new DelayBefore());
            chainFactory.Register(() => new DelayBefore());
            chainFactory.Register(() => new DelayBefore());
            chainFactory.Register(() => new DelayBefore());
            chainFactory.Register(() => new DelayBefore());
            chainFactory.Register(() => new DelayAfter());
            chainFactory.Register(() => new DelayAfter());
            chainFactory.Register(() => new DelayAfter());
            chainFactory.Register(() => new DelayAfter());
            chainFactory.Register(() => new DelayAfter());
            chainFactory.Register(() => new DelayInUsing());
            chainFactory.Register(() => new DelayBefore());
            chainFactory.Register(() => new DelayAfter());
            chainFactory.Register(() => new DelayBefore());
            chainFactory.Register(() => new DelayAfter());
            chainFactory.Register(() => new PassThrough());
            chainFactory.Register(() => new PassThrough());
            chainFactory.Register(() => new PassThrough());
            chainFactory.Register(() => new DelayInUsing());
            chainFactory.Register(() => new DelayBefore());
            chainFactory.Register(() => new DelayAfter());
            chainFactory.Register(() => new PassThrough());
            chainFactory.Register(() => new PassThrough());
            chainFactory.Register(() => new PassThrough());
            chainFactory.Register(() => new ThrowException());

            var pushMessages = new PushMessages(messages, maxConcurrency: 1);

            await pushMessages.StartAsync(tm => Connector(chainFactory, tm));

            await Task.Delay(2000);

            await pushMessages.StopAsync();
        }

        static Task Connector(ChainFactory factory, TransportMessage message)
        {
            var pipeline = factory.Create();
            var context = new IncomingContext(message);
            return pipeline.Invoke(context);
        }

        class DelayBefore : ILinkElement
        {
            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                await Task.Delay(10).ConfigureAwait(false);
                await next().ConfigureAwait(false);
            }
        }

        class DelayAfter : ILinkElement
        {
            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                await Task.Delay(10).ConfigureAwait(false);
                await next().ConfigureAwait(false);
            }
        }

        public class ThrowException : ILinkElement
        {
            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                await Task.Delay(10).ConfigureAwait(false);

                throw new InvalidOperationException(nameof(ThrowException));
            }
        }

        public class PassThrough : ILinkElement
        {
            public Task Invoke(IncomingContext context, Func<Task> next)
            {
                return next();
            }
        }

        public class DelayInUsing : ILinkElement
        {
            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await Task.Delay(10).ConfigureAwait(false);

                    await next().ConfigureAwait(false);

                    scope.Complete();
                }
            }
        }

        class LogElement : ILinkElement
        {
            private AsyncCountdownEvent countdown;

            public LogElement(AsyncCountdownEvent countdown)
            {
                this.countdown = countdown;
            }

            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                try
                {
                    await next();
                }
                catch (Exception e)
                {
                    e.StackTrace.Output();
                }
                countdown.Signal();
            }
        }
    }
}