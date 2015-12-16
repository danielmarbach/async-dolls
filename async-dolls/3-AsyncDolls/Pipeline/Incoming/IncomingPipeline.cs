using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class IncomingPipeline : IIncomingTransportStepRegisterer, IIncomingLogicalStepRegisterer, ISupportSnapshots
    {
        readonly Queue<IIncomingLogicalStep> registeredLogicalPipeline;
        readonly Queue<IIncomingTransportStep> registeredTransportPipeline;
        readonly Stack<Queue<IIncomingLogicalStep>> snapshotLogical;
        readonly Stack<Queue<IIncomingTransportStep>> snapshotTransport;
        IncomingLogicalContext currentContext;
        Queue<IIncomingLogicalStep> executingLogicalPipeline;
        Queue<IIncomingTransportStep> executingTransportPipeline;

        public IncomingPipeline()
        {
            snapshotTransport = new Stack<Queue<IIncomingTransportStep>>();
            snapshotLogical = new Stack<Queue<IIncomingLogicalStep>>();

            registeredLogicalPipeline = new Queue<IIncomingLogicalStep>();
            registeredTransportPipeline = new Queue<IIncomingTransportStep>();
        }

        public IIncomingTransportStepRegisterer Transport
        {
            get { return this; }
        }

        public IIncomingLogicalStepRegisterer Logical
        {
            get { return this; }
        }

        IIncomingLogicalStepRegisterer IIncomingLogicalStepRegisterer.Register(IIncomingLogicalStep step)
        {
            registeredLogicalPipeline.Enqueue(step);

            return this;
        }

        IIncomingLogicalStepRegisterer IIncomingLogicalStepRegisterer.Register(Func<IIncomingLogicalStep> stepFactory)
        {
            registeredLogicalPipeline.Enqueue(new LazyLogicalStep(stepFactory));

            return this;
        }

        IIncomingTransportStepRegisterer IIncomingTransportStepRegisterer.Register(IIncomingTransportStep step)
        {
            registeredTransportPipeline.Enqueue(step);

            return this;
        }

        IIncomingTransportStepRegisterer IIncomingTransportStepRegisterer.Register(Func<IIncomingTransportStep> stepFactory)
        {
            registeredTransportPipeline.Enqueue(new LazyTransportStep(stepFactory));

            return this;
        }

        public void TakeSnapshot()
        {
            snapshotLogical.Push(new Queue<IIncomingLogicalStep>(executingLogicalPipeline));
            snapshotTransport.Push(new Queue<IIncomingTransportStep>(executingTransportPipeline));
        }

        public void DeleteSnapshot()
        {
            executingLogicalPipeline = snapshotLogical.Pop();
            executingTransportPipeline = snapshotTransport.Pop();
        }

        public async Task Invoke(IBusForHandler bus, TransportMessage message, EndpointConfiguration.ReadOnly configuration)
        {
            executingTransportPipeline = new Queue<IIncomingTransportStep>(registeredTransportPipeline);
            var transportContext = new IncomingTransportContext(message, configuration);
            transportContext.SetChain(this);
            await InvokeTransport(transportContext, bus)
                .ConfigureAwait(false);

            // We assume that someone in the pipeline made logical message
            var logicalMessage = transportContext.Get<LogicalMessage>();

            executingLogicalPipeline = new Queue<IIncomingLogicalStep>(registeredLogicalPipeline);
            var logicalContext = new IncomingLogicalContext(logicalMessage, message, configuration);
            logicalContext.SetChain(this);
            currentContext = logicalContext;
            await InvokeLogical(logicalContext, bus)
                .ConfigureAwait(false);
        }

        public void DoNotInvokeAnyMoreHandlers()
        {
            currentContext.AbortHandlerInvocation();
        }

        Task InvokeLogical(IncomingLogicalContext context, IBusForHandler bus)
        {
            if (executingLogicalPipeline.Count == 0)
            {
                return Task.CompletedTask;
            }

            IIncomingLogicalStep step = executingLogicalPipeline.Dequeue();

            return step.Invoke(context, bus, () => InvokeLogical(context, bus));
        }

        Task InvokeTransport(IncomingTransportContext context, IBusForHandler bus)
        {
            if (executingTransportPipeline.Count == 0)
            {
                return Task.CompletedTask;
            }

            IIncomingTransportStep step = executingTransportPipeline.Dequeue();

            return step.Invoke(context, bus, () => InvokeTransport(context, bus));
        }

        class LazyLogicalStep : IIncomingLogicalStep
        {
            readonly Func<IIncomingLogicalStep> factory;

            public LazyLogicalStep(Func<IIncomingLogicalStep> factory)
            {
                this.factory = factory;
            }

            public Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
            {
                var step = factory();

                return step.Invoke(context, bus, next);
            }
        }

        class LazyTransportStep : IIncomingTransportStep
        {
            readonly Func<IIncomingTransportStep> factory;

            public LazyTransportStep(Func<IIncomingTransportStep> factory)
            {
                this.factory = factory;
            }

            public Task Invoke(IncomingTransportContext context, IBusForHandler bus, Func<Task> next)
            {
                var step = factory();

                return step.Invoke(context, bus, next);
            }
        }
    }
}