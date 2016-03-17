using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsyncDolls.Dequeuing;
using AsyncDolls.Pipeline;
using AsyncDolls.Pipeline.Incoming;
using AsyncDolls.Pipeline.Outgoing;

namespace AsyncDolls
{
    public class Bus : IBus
    {
        readonly EndpointConfiguration configuration;
        readonly LogicalMessageFactory factory;
        readonly IIncomingPipelineFactory incomingPipelineFactory;
        readonly IOutgoingPipelineFactory outgoingPipelineFactory;
        readonly IDequeueStrategy strategy;
        EndpointConfiguration.ReadOnly readOnlyConfiguration;

        public Bus(
            EndpointConfiguration configuration,
            IDequeueStrategy strategy,
            IOutgoingPipelineFactory outgoingPipelineFactory,
            IIncomingPipelineFactory incomingPipelineFactory)
        {
            this.incomingPipelineFactory = incomingPipelineFactory;
            this.outgoingPipelineFactory = outgoingPipelineFactory;

            factory = new LogicalMessageFactory();
            this.configuration = configuration;
            this.strategy = strategy;
        }

        public Task SendLocal(object message)
        {
            return SendLocal(message, null);
        }

        public Task Send(object message, SendOptions options = null)
        {
            return Send(message, options, null);
        }

        public Task Publish(object message, PublishOptions options = null)
        {
            return Publish(message, options, null);
        }

        public async Task StartAsync()
        {
            readOnlyConfiguration = configuration.Validate();

            await outgoingPipelineFactory.WarmupAsync();
            await incomingPipelineFactory.WarmupAsync();
            await strategy.StartAsync(readOnlyConfiguration, OnMessageAsync);
        }

        public async Task StopAsync()
        {
            await strategy.StopAsync();
            await incomingPipelineFactory.CooldownAsync();
            await outgoingPipelineFactory.CooldownAsync();
        }

        Task SendLocal(object message, TransportMessage incoming)
        {
            return Send(message, new SendOptions
            {
                Queue = readOnlyConfiguration.EndpointQueue
            }, incoming);
        }

        Task Send(object message, SendOptions options, TransportMessage incoming)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "You cannot send null");
            }

            var sendOptions = options ?? new SendOptions();
            LogicalMessage msg = factory.Create(message, sendOptions.Headers);

            return SendMessage(msg, sendOptions, incoming);
        }

        Task Publish(object message, PublishOptions options, TransportMessage incoming)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message), "You cannot publish null");
            }

            var publishOptions = options ?? new PublishOptions();
            LogicalMessage msg = factory.Create(message, publishOptions.Headers);
            publishOptions.EventType = msg.MessageType;

            return SendMessage(msg, publishOptions, incoming);
        }

        Task SendMessage(LogicalMessage outgoingLogicalMessage, DeliveryOptions options, TransportMessage incoming)
        {
            if (options.ReplyToAddress == null)
            {
                options.ReplyToAddress = configuration.EndpointQueue;
            }

            OutgoingPipeline outgoingPipeline = outgoingPipelineFactory.Create();
            return outgoingPipeline.Invoke(outgoingLogicalMessage, options, readOnlyConfiguration, incoming);
        }

        Task OnMessageAsync(TransportMessage message)
        {
            IncomingPipeline incomingPipeline = incomingPipelineFactory.Create();
            return incomingPipeline.Invoke(new IncomingBusDecorator(this, incomingPipeline, message), message, readOnlyConfiguration);
        }

        class IncomingBusDecorator : IBusForHandler
        {
            readonly Bus bus;
            readonly TransportMessage incoming;
            readonly IncomingPipeline incomingPipeline;

            public IncomingBusDecorator(Bus bus, IncomingPipeline incomingPipeline, TransportMessage incoming)
            {
                this.incoming = incoming;
                this.incomingPipeline = incomingPipeline;
                this.bus = bus;
            }

            public Task SendLocal(object message)
            {
                return bus.SendLocal(message, incoming);
            }

            public Task Send(object message, SendOptions options = null)
            {
                return bus.Send(message, options, incoming);
            }

            public Task Publish(object message, PublishOptions options = null)
            {
                return bus.Publish(message, options, incoming);
            }

            public Task Reply(object message, ReplyOptions options = null)
            {
                ReplyOptions replyOptions = GetOrCreateReplyOptions(incoming, options);
                return bus.Send(message, replyOptions, incoming);
            }

            public IDictionary<string, string> Headers(object message)
            {
                return incoming.Headers;
            }

            public void DoNotContinueDispatchingCurrentMessageToHandlers()
            {
                incomingPipeline.DoNotInvokeAnyMoreHandlers();
            }

            static ReplyOptions GetOrCreateReplyOptions(TransportMessage incoming, ReplyOptions options = null)
            {
                Queue destination = incoming.ReplyTo;

                string correlationId = !string.IsNullOrEmpty(incoming.CorrelationId)
                    ? incoming.CorrelationId
                    : incoming.Id;

                if (options == null)
                {
                    return new ReplyOptions(destination, correlationId);
                }

                options.Queue = options.Queue ?? destination;
                options.CorrelationId = options.CorrelationId ?? correlationId;
                return options;
            }
        }
    }
}