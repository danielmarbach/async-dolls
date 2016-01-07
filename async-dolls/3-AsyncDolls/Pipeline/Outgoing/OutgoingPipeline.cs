using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public class OutgoingPipeline : IOutgoingTransportStepRegisterer, IOutgoingLogicalStepRegisterer
    {
        readonly Queue<IOutgoingLogicalStep> registeredlogicalPipelineSteps;
        readonly Queue<IOutgoingTransportStep> registeredTransportPipelineSteps;
        readonly Stack<Queue<IOutgoingLogicalStep>> snapshotLogical;
        readonly Stack<Queue<IOutgoingTransportStep>> snapshotTransport;
        Queue<IOutgoingLogicalStep> executingLogicalPipeline;
        Queue<IOutgoingTransportStep> executingTransportPipeline;

        public OutgoingPipeline()
        {
            snapshotTransport = new Stack<Queue<IOutgoingTransportStep>>();
            snapshotLogical = new Stack<Queue<IOutgoingLogicalStep>>();

            registeredlogicalPipelineSteps = new Queue<IOutgoingLogicalStep>();
            registeredTransportPipelineSteps = new Queue<IOutgoingTransportStep>();
        }

        public IOutgoingTransportStepRegisterer Transport
        {
            get { return this; }
        }

        public IOutgoingLogicalStepRegisterer Logical
        {
            get { return this; }
        }

        IOutgoingLogicalStepRegisterer IOutgoingLogicalStepRegisterer.Register(IOutgoingLogicalStep step)
        {
            registeredlogicalPipelineSteps.Enqueue(step);

            return this;
        }

        IOutgoingLogicalStepRegisterer IOutgoingLogicalStepRegisterer.Register(Func<IOutgoingLogicalStep> stepFactory)
        {
            registeredlogicalPipelineSteps.Enqueue(new LazyLogicalStep(stepFactory));

            return this;
        }

        IOutgoingTransportStepRegisterer IOutgoingTransportStepRegisterer.Register(IOutgoingTransportStep step)
        {
            registeredTransportPipelineSteps.Enqueue(step);

            return this;
        }

        IOutgoingTransportStepRegisterer IOutgoingTransportStepRegisterer.Register(Func<IOutgoingTransportStep> stepFactory)
        {
            registeredTransportPipelineSteps.Enqueue(new LazyTransportStep(stepFactory));

            return this;
        }

        public void TakeSnapshot()
        {
            snapshotLogical.Push(new Queue<IOutgoingLogicalStep>(executingLogicalPipeline));
            snapshotTransport.Push(new Queue<IOutgoingTransportStep>(executingTransportPipeline));
        }

        public void DeleteSnapshot()
        {
            executingLogicalPipeline = snapshotLogical.Pop();
            executingTransportPipeline = snapshotTransport.Pop();
        }

        public virtual async Task Invoke(LogicalMessage outgoingLogicalMessage, DeliveryOptions options, EndpointConfiguration.ReadOnly configuration, TransportMessage incomingTransportMessage = null)
        {
            executingLogicalPipeline = new Queue<IOutgoingLogicalStep>(registeredlogicalPipelineSteps);
            var logicalContext = new OutgoingLogicalContext(outgoingLogicalMessage, options, configuration);
            await InvokeLogical(logicalContext)
                .ConfigureAwait(false);

            // We assume that someone in the pipeline made transport message
            var outgoingTransportMessage = logicalContext.Get<TransportMessage>();

            executingTransportPipeline = new Queue<IOutgoingTransportStep>(registeredTransportPipelineSteps);
            var transportContext = new OutgoingTransportContext(outgoingLogicalMessage, outgoingTransportMessage, options, configuration, incomingTransportMessage);
            await InvokeTransport(transportContext)
                .ConfigureAwait(false);
        }

        Task InvokeLogical(OutgoingLogicalContext context)
        {
            if (executingLogicalPipeline.Count == 0)
            {
                return Task.CompletedTask;
            }

            IOutgoingLogicalStep step = executingLogicalPipeline.Dequeue();

            return step.Invoke(context, () => InvokeLogical(context));
        }

        Task InvokeTransport(OutgoingTransportContext context)
        {
            if (executingTransportPipeline.Count == 0)
            {
                return Task.CompletedTask;
            }

            IOutgoingTransportStep step = executingTransportPipeline.Dequeue();

            return step.Invoke(context, () => InvokeTransport(context));
        }

        class LazyLogicalStep : IOutgoingLogicalStep
        {
            readonly Func<IOutgoingLogicalStep> factory;

            public LazyLogicalStep(Func<IOutgoingLogicalStep> factory)
            {
                this.factory = factory;
            }

            public Task Invoke(OutgoingLogicalContext context, Func<Task> next)
            {
                var step = factory();

                return step.Invoke(context, next);
            }
        }

        class LazyTransportStep : IOutgoingTransportStep
        {
            readonly Func<IOutgoingTransportStep> factory;

            public LazyTransportStep(Func<IOutgoingTransportStep> factory)
            {
                this.factory = factory;
            }

            public Task Invoke(OutgoingTransportContext context, Func<Task> next)
            {
                var step = factory();

                return step.Invoke(context, next);
            }
        }
    }
}