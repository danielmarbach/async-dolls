using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncDolls.AsyncStateWithDolls
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
            chainFactory.Register(() => new DelayElement());
            chainFactory.Register(() => new LogElement());
            chainFactory.Register(() => new DecrementElement(countdown));

            var pushMessages = new PushMessages(messages, maxConcurrency: 1);

            await pushMessages.StartAsync(tm => Connector(chainFactory, tm));

            await countdown.WaitAsync();

            await pushMessages.StopAsync();
        }

        static Task Connector(ChainFactory factory, TransportMessage message)
        {
            var pipeline = factory.Create();
            var context = new IncomingContext(message);
            return pipeline.Invoke(context);
        }

        class DelayElement : ILinkElement
        {
            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                await next().ConfigureAwait(false);
            }
        }

        class LogElement : ILinkElement
        {
            public Task Invoke(IncomingContext context, Func<Task> next)
            {
                context.Message.Id.Output();
                return next();
            }
        }

        class DecrementElement : ILinkElement
        {
            private readonly AsyncCountdownEvent countdown;

            public DecrementElement(AsyncCountdownEvent countdown)
            {
                this.countdown = countdown;
            }

            public Task Invoke(IncomingContext context, Func<Task> next)
            {
                countdown.Signal();
                return next();
            }
        }
    }
}