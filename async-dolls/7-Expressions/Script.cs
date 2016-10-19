using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace AsyncDolls.Expressions
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

        class DelayBefore : ILinkElement<IncomingContext, IncomingContext>
        {
            public async Task Invoke(IncomingContext context, Func<IncomingContext, Task> next)
            {
                await Task.Delay(10).ConfigureAwait(false);
                await next(context).ConfigureAwait(false);
            }
        }

        class DelayAfter : ILinkElement<IncomingContext, IncomingContext>
        {
            public async Task Invoke(IncomingContext context, Func<IncomingContext, Task> next)
            {
                await Task.Delay(10).ConfigureAwait(false);
                await next(context).ConfigureAwait(false);
            }
        }

        public class ThrowException : ILinkElement<IncomingContext, IncomingContext>
        {
            public async Task Invoke(IncomingContext context, Func<IncomingContext, Task> next)
            {
                await Task.Delay(10).ConfigureAwait(false);

                throw new InvalidOperationException(nameof(ThrowException));
            }
        }

        public class PassThrough : ILinkElement<IncomingContext, IncomingContext>
        {
            public Task Invoke(IncomingContext context, Func<IncomingContext, Task> next)
            {
                return next(context);
            }
        }

        public class DelayInUsing : ILinkElement<IncomingContext, IncomingContext>
        {
            public async Task Invoke(IncomingContext context, Func<IncomingContext, Task> next)
            {
                using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await Task.Delay(10).ConfigureAwait(false);

                    await next(context).ConfigureAwait(false);

                    scope.Complete();
                }
            }
        }

        class LogElement : ILinkElement<IncomingContext, IncomingContext>
        {
            private AsyncCountdownEvent countdown;

            public LogElement(AsyncCountdownEvent countdown)
            {
                this.countdown = countdown;
            }

            public async Task Invoke(IncomingContext context, Func<IncomingContext, Task> next)
            {
                try
                {
                    await next(context).ConfigureAwait(false);
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