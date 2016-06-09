using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using AsyncDolls.AsyncDollsInDepth;
using NUnit.Framework;

namespace AsyncDolls.AsyncDollsPartial
{
    [TestFixture]
    public class Script
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
            chainFactory.Register(() => new SignalElement(countdown));
            chainFactory.Register(() => new SwallowElement());
            chainFactory.Register(() => new DelayPhysicalBefore());
            chainFactory.Register(() => new DelayPhysicalAfter());
            chainFactory.Register(() => new DelayPhysicalBefore());
            chainFactory.Register(() => new DelayPhysicalAfter());
            chainFactory.Register(() => new DelayPhysicalBefore());
            chainFactory.Register(() => new DelayPhysicalAfter());
            chainFactory.Register(() => new DelayPhysicalBefore());
            chainFactory.Register(() => new DelayPhysicalAfter());
            chainFactory.Register(() => new LogPhysical());
            chainFactory.Register(() => new PhysicalToLogicalConnector());
            chainFactory.Register(() => new DelayLogicalBefore());
            chainFactory.Register(() => new DelayLogicalAfter());
            chainFactory.Register(() => new DelayLogicalBefore());
            chainFactory.Register(() => new DelayLogicalAfter());
            chainFactory.Register(() => new DelayLogicalBefore());
            chainFactory.Register(() => new DelayLogicalAfter());
            chainFactory.Register(() => new DelayLogicalBefore());
            chainFactory.Register(() => new DelayLogicalAfter());
            chainFactory.Register(() => new DelayLogicalBefore());
            chainFactory.Register(() => new DelayLogicalAfter());
            chainFactory.Register(() => new DelayLogicalBefore());
            chainFactory.Register(() => new DelayLogicalAfter());
            chainFactory.Register(() => new DelayLogicalBefore());
            chainFactory.Register(() => new DelayLogicalAfter());
            chainFactory.Register(() => new DelayLogicalBefore());
            chainFactory.Register(() => new DelayLogicalAfter());
            chainFactory.Register(() => new LogLogical());
            chainFactory.Register(() => new ThrowElement());

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

        class DelayPhysicalBefore : BeforeElement<IncomingPhysicalContext>
        {
            public override async Task Invoke(IncomingPhysicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        class DelayPhysicalAfter : AfterElement<IncomingPhysicalContext>
        {
            public override async Task Invoke(IncomingPhysicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        class LogPhysical : BeforeElement<IncomingPhysicalContext>
        {
            public override Task Invoke(IncomingPhysicalContext context)
            {
                context.Message.Id.Output();
                return Task.CompletedTask;
            }
        }

        class PhysicalToLogicalConnector : ElementConnector<IncomingPhysicalContext, IncomingLogicalContext>
        {
            public override Task Invoke(IncomingPhysicalContext context, Func<IncomingLogicalContext, Task> next)
            {
                return next(new IncomingLogicalContext(new LogicalMessage(), context));
            }
        }

        class DelayLogicalBefore : BeforeElement<IncomingLogicalContext>
        {
            public override async Task Invoke(IncomingLogicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        class DelayLogicalAfter : AfterElement<IncomingLogicalContext>
        {
            public override async Task Invoke(IncomingLogicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        class LogLogical : BeforeElement<IncomingLogicalContext>
        {
            public override Task Invoke(IncomingLogicalContext context)
            {
                context.Message.Instance.ToString().Output();
                return Task.CompletedTask;
            }
        }

        class SignalElement : AfterElement<IncomingPhysicalContext>
        {
            private readonly AsyncCountdownEvent countdown;

            public SignalElement(AsyncCountdownEvent countdown)
            {
                this.countdown = countdown;
            }

            public override Task Invoke(IncomingPhysicalContext context)
            {
                countdown.Signal();
                return Task.CompletedTask;
            }
        }

        class SwallowElement : SurroundElement<IncomingPhysicalContext>
        {
            public override async Task Invoke(IncomingPhysicalContext context, Func<Task> next)
            {
                try
                {
                    await next().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    e.StackTrace.Output();
                }
            }
        }

        class ThrowElement : BeforeElement<IncomingLogicalContext>
        {
            public override async Task Invoke(IncomingLogicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
                throw new InvalidOperationException();
            }
        }
    }
}