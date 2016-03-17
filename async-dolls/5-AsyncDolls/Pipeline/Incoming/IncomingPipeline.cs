using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class IncomingPipeline : IIncomingTransportStepRegisterer, IIncomingLogicalStepRegisterer
    {
        readonly List<IIncomingLogicalStep> registeredLogicalPipeline;
        readonly List<IIncomingTransportStep> registeredTransportPipeline;
        IncomingLogicalContext currentContext;

        public IncomingPipeline()
        {
            registeredLogicalPipeline = new List<IIncomingLogicalStep>();
            registeredTransportPipeline = new List<IIncomingTransportStep>();
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
            registeredLogicalPipeline.Add(step);

            return this;
        }

        IIncomingLogicalStepRegisterer IIncomingLogicalStepRegisterer.Register(Func<IIncomingLogicalStep> stepFactory)
        {
            registeredLogicalPipeline.Add(new LazyLogicalStep(stepFactory));

            return this;
        }

        IIncomingTransportStepRegisterer IIncomingTransportStepRegisterer.Register(IIncomingTransportStep step)
        {
            registeredTransportPipeline.Add(step);

            return this;
        }

        IIncomingTransportStepRegisterer IIncomingTransportStepRegisterer.Register(Func<IIncomingTransportStep> stepFactory)
        {
            registeredTransportPipeline.Add(new LazyTransportStep(stepFactory));

            return this;
        }

        public async Task Invoke(IBusForHandler bus, TransportMessage message, EndpointConfiguration.ReadOnly configuration)
        {
            var transportContext = new IncomingTransportContext(message, configuration);
            await InvokeTransport(transportContext, bus)
                .ConfigureAwait(false);

            // We assume that someone in the pipeline made logical message
            var logicalMessage = transportContext.Get<LogicalMessage>();

            var logicalContext = new IncomingLogicalContext(logicalMessage, message, configuration);
            currentContext = logicalContext;
            await InvokeLogical(logicalContext, bus)
                .ConfigureAwait(false);
        }

        public void DoNotInvokeAnyMoreHandlers()
        {
            currentContext.AbortHandlerInvocation();
        }

        Task InvokeLogical(IncomingLogicalContext context, IBusForHandler bus, int currentIndex = 0)
        {
            if (currentIndex == registeredLogicalPipeline.Count)
            {
                return Task.CompletedTask;
            }

            IIncomingLogicalStep step = registeredLogicalPipeline[currentIndex];

            return step.Invoke(context, bus, () => InvokeLogical(context, bus, currentIndex + 1));
        }

        Task InvokeTransport(IncomingTransportContext context, IBusForHandler bus, int currentIndex = 0)
        {
            if (currentIndex == registeredTransportPipeline.Count)
            {
                return Task.CompletedTask;
            }

            IIncomingTransportStep step = registeredTransportPipeline[currentIndex];

            return step.Invoke(context, bus, () => InvokeTransport(context, bus, currentIndex + 1));
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