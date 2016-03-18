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

            var pipelineFactory = new IncomingPipelineFactory();
            pipelineFactory.Register(() => new SignalStep(countdown));
            pipelineFactory.Register(() => new SwallowStep());
            pipelineFactory.Register(() => new DelayPhysicalBefore());
            pipelineFactory.Register(() => new DelayPhysicalAfter());
            pipelineFactory.Register(() => new DelayPhysicalBefore());
            pipelineFactory.Register(() => new DelayPhysicalAfter());
            pipelineFactory.Register(() => new DelayPhysicalBefore());
            pipelineFactory.Register(() => new DelayPhysicalAfter());
            pipelineFactory.Register(() => new DelayPhysicalBefore());
            pipelineFactory.Register(() => new DelayPhysicalAfter());
            pipelineFactory.Register(() => new LogPhysical());
            pipelineFactory.Register(() => new PhysicalToLogicalConnector());
            pipelineFactory.Register(() => new DelayLogicalBefore());
            pipelineFactory.Register(() => new DelayLogicalAfter());
            pipelineFactory.Register(() => new DelayLogicalBefore());
            pipelineFactory.Register(() => new DelayLogicalAfter());
            pipelineFactory.Register(() => new DelayLogicalBefore());
            pipelineFactory.Register(() => new DelayLogicalAfter());
            pipelineFactory.Register(() => new DelayLogicalBefore());
            pipelineFactory.Register(() => new DelayLogicalAfter());
            pipelineFactory.Register(() => new DelayLogicalBefore());
            pipelineFactory.Register(() => new DelayLogicalAfter());
            pipelineFactory.Register(() => new DelayLogicalBefore());
            pipelineFactory.Register(() => new DelayLogicalAfter());
            pipelineFactory.Register(() => new DelayLogicalBefore());
            pipelineFactory.Register(() => new DelayLogicalAfter());
            pipelineFactory.Register(() => new DelayLogicalBefore());
            pipelineFactory.Register(() => new DelayLogicalAfter());
            pipelineFactory.Register(() => new LogLogical());
            pipelineFactory.Register(() => new ThrowStep());

            var strategy = new PushMessages(messages, maxConcurrency: 1);

            await strategy.StartAsync(tm => Connector(pipelineFactory, tm));

            await countdown.WaitAsync();

            await strategy.StopAsync();
        }

        static Task Connector(IncomingPipelineFactory factory, TransportMessage message)
        {
            var pipeline = factory.Create();
            var context = new IncomingPhysicalContext(message);
            return pipeline.Invoke(context);
        }

        class DelayPhysicalBefore : BeforeStep<IncomingPhysicalContext>
        {
            public override async Task Invoke(IncomingPhysicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        class DelayPhysicalAfter : AfterStep<IncomingPhysicalContext>
        {
            public override async Task Invoke(IncomingPhysicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        class LogPhysical : BeforeStep<IncomingPhysicalContext>
        {
            public override Task Invoke(IncomingPhysicalContext context)
            {
                context.Message.Id.Output();
                return Task.CompletedTask;
            }
        }

        class PhysicalToLogicalConnector : StepConnector<IncomingPhysicalContext, IncomingLogicalContext>
        {
            public override Task Invoke(IncomingPhysicalContext context, Func<IncomingLogicalContext, Task> next)
            {
                return next(new IncomingLogicalContext(new LogicalMessage(), context));
            }
        }

        class DelayLogicalBefore : BeforeStep<IncomingLogicalContext>
        {
            public override async Task Invoke(IncomingLogicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        class DelayLogicalAfter : AfterStep<IncomingLogicalContext>
        {
            public override async Task Invoke(IncomingLogicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
            }
        }

        class LogLogical : BeforeStep<IncomingLogicalContext>
        {
            public override Task Invoke(IncomingLogicalContext context)
            {
                context.Message.Instance.ToString().Output();
                return Task.CompletedTask;
            }
        }

        class SignalStep : AfterStep<IncomingPhysicalContext>
        {
            private readonly AsyncCountdownEvent countdown;

            public SignalStep(AsyncCountdownEvent countdown)
            {
                this.countdown = countdown;
            }

            public override Task Invoke(IncomingPhysicalContext context)
            {
                countdown.Signal();
                return Task.CompletedTask;
            }
        }

        class SwallowStep : SurroundStep<IncomingPhysicalContext>
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

        class ThrowStep : BeforeStep<IncomingLogicalContext>
        {
            public override async Task Invoke(IncomingLogicalContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);
                throw new InvalidOperationException();
            }
        }
    }
}