using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    [TestFixture]
    public class AsyncStateWithDollsTypedScript
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
            chainFactory.Register(() => new PhysicalToLogicalConnector());
            chainFactory.Register(() => new DecrementElement(countdown));

            var pushMessages = new PushMessages(messages, maxConcurrency: 1);

            await pushMessages.StartAsync(tm => Connector(chainFactory, tm));

            await countdown.WaitAsync();

            await pushMessages.StopAsync();
        }

        static Task Connector(ChainFactory factory, TransportMessage message)
        {
            var pipeline = factory.Create();
            var context = new IncomingPhysicalContext(message);
            return pipeline.Invoke(context);
        }

        class DelayElement : LinkElement<IncomingPhysicalContext>
        {
            public override async Task Invoke(IncomingPhysicalContext context, Func<Task> next)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                await next().ConfigureAwait(false);
            }
        }

        class LogElement : LinkElement<IncomingPhysicalContext>
        {
            public override Task Invoke(IncomingPhysicalContext context, Func<Task> next)
            {
                context.Message.Id.Output();
                return next();
            }
        }

        class PhysicalToLogicalConnector : ElementConnector<IncomingPhysicalContext, IncomingLogicalContext>
        {
            public override Task Invoke(IncomingPhysicalContext context, Func<IncomingLogicalContext, Task> next)
            {
                return next(new IncomingLogicalContext(new LogicalMessage(), context));
            }
        }

        class DecrementElement : LinkElement<IncomingLogicalContext>
        {
            private readonly AsyncCountdownEvent countdown;

            public DecrementElement(AsyncCountdownEvent countdown)
            {
                this.countdown = countdown;
            }

            public override Task Invoke(IncomingLogicalContext context, Func<Task> next)
            {
                context.Message.Instance.ToString().Output();
                countdown.Signal();
                return next();
            }
        }
    }
}