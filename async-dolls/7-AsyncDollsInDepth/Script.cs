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

            var pipelineFactory = new IncomingPipelineFactory();
            pipelineFactory.Register(() => new LogStep(countdown));
            pipelineFactory.Register(() => new DelayBefore());
            pipelineFactory.Register(() => new DelayBefore());
            pipelineFactory.Register(() => new DelayBefore());
            pipelineFactory.Register(() => new DelayBefore());
            pipelineFactory.Register(() => new DelayBefore());
            pipelineFactory.Register(() => new DelayBefore());
            pipelineFactory.Register(() => new DelayAfter());
            pipelineFactory.Register(() => new DelayAfter());
            pipelineFactory.Register(() => new DelayAfter());
            pipelineFactory.Register(() => new DelayAfter());
            pipelineFactory.Register(() => new DelayAfter());
            pipelineFactory.Register(() => new DelayInUsing());
            pipelineFactory.Register(() => new DelayBefore());
            pipelineFactory.Register(() => new DelayAfter());
            pipelineFactory.Register(() => new DelayBefore());
            pipelineFactory.Register(() => new DelayAfter());
            pipelineFactory.Register(() => new PassThrough());
            pipelineFactory.Register(() => new PassThrough());
            pipelineFactory.Register(() => new PassThrough());
            pipelineFactory.Register(() => new DelayInUsing());
            pipelineFactory.Register(() => new DelayBefore());
            pipelineFactory.Register(() => new DelayAfter());
            pipelineFactory.Register(() => new PassThrough());
            pipelineFactory.Register(() => new PassThrough());
            pipelineFactory.Register(() => new PassThrough());
            pipelineFactory.Register(() => new ThrowException());

            var strategy = new PushMessages(messages, maxConcurrency: 1);

            await strategy.StartAsync(tm => Connector(pipelineFactory, tm));

            await Task.Delay(2000);

            await strategy.StopAsync();
        }

        static Task Connector(IncomingPipelineFactory factory, TransportMessage message)
        {
            var pipeline = factory.Create();
            var context = new IncomingContext(message);
            return pipeline.Invoke(context);
        }

        class DelayBefore : IIncomingStep
        {
            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                await Task.Delay(10).ConfigureAwait(false);
                await next().ConfigureAwait(false);
            }
        }

        class DelayAfter : IIncomingStep
        {
            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                await Task.Delay(10).ConfigureAwait(false);
                await next().ConfigureAwait(false);
            }
        }

        public class ThrowException : IIncomingStep
        {
            public async Task Invoke(IncomingContext context, Func<Task> next)
            {
                await Task.Delay(10).ConfigureAwait(false);

                throw new InvalidOperationException(nameof(ThrowException));
            }
        }

        public class PassThrough : IIncomingStep
        {
            public Task Invoke(IncomingContext context, Func<Task> next)
            {
                return next();
            }
        }

        public class DelayInUsing : IIncomingStep
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

        class LogStep : IIncomingStep
        {
            private AsyncCountdownEvent countdown;

            public LogStep(AsyncCountdownEvent countdown)
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