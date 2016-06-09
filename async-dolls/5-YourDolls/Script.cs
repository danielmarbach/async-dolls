using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncDolls.YourDolls
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

        static Task Connector(ChainFactory factory, TransportMessage transportMessage)
        {
            var pipeline = factory.Create();
            return pipeline.Invoke(transportMessage);
        }

        class DelayElement : ILinkElement
        {
            public async Task Invoke(TransportMessage transportMessage, Func<Task> next)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                await next().ConfigureAwait(false);
            }
        }

        class LogElement : ILinkElement
        {
            public Task Invoke(TransportMessage transportMessage, Func<Task> next)
            {
                transportMessage.Id.Output();
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

            public Task Invoke(TransportMessage transportMessage, Func<Task> next)
            {
                countdown.Signal();
                return next();
            }
        }
    }
}