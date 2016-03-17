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

            var pipelineFactory = new IncomingPipelineFactory();
            pipelineFactory.Register(() => new DelayStep());
            pipelineFactory.Register(() => new LogStep());
            pipelineFactory.Register(() => new DecrementStep(countdown));

            var strategy = new PushMessages(messages, maxConcurrency: 1);

            await strategy.StartAsync(tm => Connector(pipelineFactory, tm));

            await countdown.WaitAsync();

            await strategy.StopAsync();
        }

        static Task Connector(IncomingPipelineFactory factory, TransportMessage message)
        {
            var pipeline = factory.Create();
            var context = new IncomingContext(message);
            return pipeline.Invoke(context);
        }

        class DelayStep : IIncomingStep
        {
            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                await next().ConfigureAwait(false);
            }
        }

        class LogStep : IIncomingStep
        {
            public Task Invoke(IncomingContext context, Func<Task> next)
            {
                context.Message.Id.Output();
                return next();
            }
        }

        class DecrementStep : IIncomingStep
        {
            private readonly AsyncCountdownEvent countdown;

            public DecrementStep(AsyncCountdownEvent countdown)
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