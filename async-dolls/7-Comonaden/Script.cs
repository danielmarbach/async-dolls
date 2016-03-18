using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace AsyncDolls.Comonaden
{
    // http://blogs.msdn.com/b/pfxteam/archive/2013/04/03/tasks-monads-and-linq.aspx
    // https://github.com/iSynaptic/Monad-Comonad-Precis/blob/master/Precis.cs

    [TestFixture]
    // This is just a playground. In practice this is highly innefficient.
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
            public async Task<Continuation> Invoke(IncomingContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);

                return Continuation.Empty;
            }
        }

        class DelayAfter : IIncomingStep
        {
            public Task<Continuation> Invoke(IncomingContext context)
            {
                return new Continuation
                {
                    After = () => Task.Delay(10)
                };
            }
        }

        public class ThrowException : IIncomingStep
        {
            public async Task<Continuation> Invoke(IncomingContext context)
            {
                await Task.Delay(10).ConfigureAwait(false);

                throw new InvalidOperationException(nameof(ThrowException));
            }
        }

        public class PassThrough : IIncomingStep
        {
            public Task<Continuation> Invoke(IncomingContext context)
            {
                return Continuation.Empty;
            }
        }

        public class DelayInUsing : IIncomingStep
        {
            public async Task<Continuation> Invoke(IncomingContext context)
            {
                var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                await Task.Delay(10).ConfigureAwait(false);

                return new Continuation
                {
                    After = () => { scope.Complete(); return Task.CompletedTask; },
                    Finally = () => { scope.Dispose(); return Task.CompletedTask; }
                };
            }
        }

        class LogStep : IIncomingStep
        {
            private AsyncCountdownEvent countdown;

            public LogStep(AsyncCountdownEvent countdown)
            {
                this.countdown = countdown;
            }

            public Task<Continuation> Invoke(IncomingContext context)
            {
                return new Continuation
                {
                    Catch = async info =>
                    {
                        info.SourceException.StackTrace.Output();
                        countdown.Signal();
                        return null;
                    }
                };
            }
        }
    }
}