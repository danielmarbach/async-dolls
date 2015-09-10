namespace AsyncDolls.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AsyncDolls;
    using AsyncDolls.Dequeuing;
    using AsyncDolls.Pipeline;
    using AsyncDolls.Pipeline.Incoming;
    using AsyncDolls.Pipeline.Outgoing;

    public class MessageUnit : IBus
    {
        readonly EndpointConfiguration configuration;
        readonly List<LogicalMessage> incomingLogical;
        readonly List<TransportMessage> incomingTransport;
        readonly List<TransportMessage> deadLetter;
        readonly List<LogicalMessage> outgoingLogical;
        readonly List<TransportMessage> outgoingTransport;
        Func<TransportMessage, Task> outgoing;
        MessageReceiverSimulator simulator;
        Bus unit;

        public MessageUnit(EndpointConfiguration configuration)
        {
            this.configuration = configuration;

            outgoingTransport = new List<TransportMessage>();
            incomingTransport = new List<TransportMessage>();
            deadLetter = new List<TransportMessage>();
            incomingLogical = new List<LogicalMessage>();
            outgoingLogical = new List<LogicalMessage>();
        }

        public Address Endpoint => configuration.EndpointQueue;

        public List<TransportMessage> DeadLetter => deadLetter;

        public List<TransportMessage> OutgoingTransport => outgoingTransport;

        public List<TransportMessage> IncomingTransport => incomingTransport;

        public List<LogicalMessage> IncomingLogical => incomingLogical;

        public List<LogicalMessage> OutgoingLogical => outgoingLogical;

        protected IHandlerRegistry Registry { get; set; }
        protected IMessageRouter Router { get; set; }

        public Task SendLocal(object message)
        {
            return unit.SendLocal(message);
        }

        public Task Send(object message, SendOptions options = null)
        {
            return unit.Send(message, options);
        }

        public Task Publish(object message, PublishOptions options = null)
        {
            return unit.Publish(message, options);
        }

        public MessageUnit Use(HandlerRegistry registry)
        {
            Registry = registry;
            return this;
        }

        public MessageUnit Use(IMessageRouter router)
        {
            Router = router;
            return this;
        }

        public Task StartAsync()
        {
            simulator = new MessageReceiverSimulator(incomingTransport);
            IOutgoingPipelineFactory outgoingFactory = CreateOutgoingPipelineFactory();
            IIncomingPipelineFactory incomingFactory = CreateIncomingPipelineFactory();

            unit = CreateBus(simulator, outgoingFactory, incomingFactory);
            return unit.StartAsync();
        }

        public Task StopAsync()
        {
            return unit.StopAsync();
        }

        public void SetOutgoing(Func<TransportMessage, Task> outgoing)
        {
            this.outgoing = msg =>
            {
                outgoingTransport.Add(msg);
                return outgoing(msg);
            };
        }

        public Task HandOver(TransportMessage message)
        {
            return simulator.HandOver(message);
        }

        protected virtual IIncomingPipelineFactory CreateIncomingPipelineFactory()
        {
            return new UnitIncomingPipelineFactory(Registry, IncomingLogical, DeadLetter);
        }

        protected virtual IOutgoingPipelineFactory CreateOutgoingPipelineFactory()
        {
            return new UnitOutgoingPipelineFactory(outgoing, OutgoingLogical, Router);
        }

        protected virtual Bus CreateBus(IReceiveMessages receiver, IOutgoingPipelineFactory outgoingPipelineFactory, IIncomingPipelineFactory incomingPipelineFactory)
        {
            return new Bus(configuration, new DequeueStrategy(receiver), outgoingPipelineFactory, incomingPipelineFactory);
        }

        class UnitOutgoingPipelineFactory : IOutgoingPipelineFactory
        {
            readonly Func<TransportMessage, Task> onMessage;
            readonly ICollection<LogicalMessage> outgoing;
            readonly IMessageRouter router;

            public UnitOutgoingPipelineFactory(Func<TransportMessage, Task> onMessage, ICollection<LogicalMessage> outgoing, IMessageRouter router)
            {
                this.router = router;
                this.onMessage = onMessage;
                this.outgoing = outgoing;
            }

            public Task WarmupAsync()
            {
                return Task.FromResult(0);
            }

            public OutgoingPipeline Create()
            {
                var pipeline = new OutgoingPipeline();
                var senderStep = new DispatchToTransportStep(new MessageSenderSimulator(onMessage), new MessagePublisherSimulator(onMessage));

                pipeline.Logical
                    .Register(new CreateTransportMessageStep())
                    .Register(new TraceOutgoingLogical(outgoing));

                pipeline.Transport
                    .Register(new SerializeMessageStep(new NewtonsoftJsonMessageSerializer()))
                    .Register(new DetermineDestinationStep(router))
                    .Register(new EnrichTransportMessageWithDestinationAddress())
                    .Register(senderStep);

                return pipeline;
            }

            public Task CooldownAsync()
            {
                return Task.FromResult(0);
            }
        }

        class UnitIncomingPipelineFactory : IIncomingPipelineFactory
        {
            readonly ICollection<TransportMessage> deadLetter;
            readonly ICollection<LogicalMessage> incoming;
            readonly IHandlerRegistry registry;

            public UnitIncomingPipelineFactory(IHandlerRegistry registry, ICollection<LogicalMessage> incoming, ICollection<TransportMessage> deadLetter)
            {
                this.incoming = incoming;
                this.deadLetter = deadLetter;
                this.registry = registry;
            }

            public Task WarmupAsync()
            {
                return Task.FromResult(0);
            }

            public IncomingPipeline Create()
            {
                var pipeline = new IncomingPipeline();

                pipeline.Transport
                    .Register(new DeadLetterMessagesWhichCantBeDeserializedStep(new TraceDeadLetter(deadLetter)))
                    .Register(new DeserializeTransportMessageStep(new NewtonsoftJsonMessageSerializer()));

                pipeline.Logical
                    .Register(new RetryMessagesStep())
                    .Register(new DeadLetterMessagesWhenRetryCountIsReachedStep(new TraceDeadLetter(deadLetter)))
                    .Register(new LoadMessageHandlersStep(registry))
                    .Register(new InvokeHandlerStep())
                    .Register(new TraceIncomingLogical(incoming));

                return pipeline;
            }

            public Task CooldownAsync()
            {
                return Task.FromResult(0);
            }
        }

        class MessageSenderSimulator : ISendMessages
        {
            readonly Func<TransportMessage, Task> onMessage;

            public MessageSenderSimulator(Func<TransportMessage, Task> onMessage)
            {
                this.onMessage = onMessage;
            }

            public Task SendAsync(TransportMessage message, SendOptions options)
            {
                var transportMessage = new TransportMessage(message);

                return onMessage(transportMessage);
            }
        }

        class MessagePublisherSimulator : IPublishMessages
        {
            readonly Func<TransportMessage, Task> onMessage;

            public MessagePublisherSimulator(Func<TransportMessage, Task> onMessage)
            {
                this.onMessage = onMessage;
            }

            public Task PublishAsync(TransportMessage message, PublishOptions options)
            {
                var transportMessage = new TransportMessage(message);

                return onMessage(transportMessage);
            }
        }

        class MessageReceiverSimulator : IReceiveMessages
        {
            readonly ICollection<TransportMessage> collector;
            Func<TransportMessage, Task> onMessage;

            public MessageReceiverSimulator(ICollection<TransportMessage> collector)
            {
                this.collector = collector;
            }

            public Task<AsyncClosable> StartAsync(EndpointConfiguration.ReadOnly configuration, Func<TransportMessage, Task> onMessage)
            {
                this.onMessage = onMessage;

                return Task.FromResult(new AsyncClosable(() => Task.FromResult(0)));
            }

            public Task HandOver(TransportMessage message)
            {
                collector.Add(message);
                return onMessage(message);
            }
        }

        class TraceIncomingLogical : IIncomingLogicalStep
        {
            readonly ICollection<LogicalMessage> collector;

            public TraceIncomingLogical(ICollection<LogicalMessage> collector)
            {
                this.collector = collector;
            }

            public Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
            {
                collector.Add(context.LogicalMessage);
                return next();
            }
        }

        class TraceOutgoingLogical : IOutgoingLogicalStep
        {
            readonly ICollection<LogicalMessage> collector;

            public TraceOutgoingLogical(ICollection<LogicalMessage> collector)
            {
                this.collector = collector;
            }

            public Task Invoke(OutgoingLogicalContext context, Func<Task> next)
            {
                collector.Add(context.LogicalMessage);
                return next();
            }
        }

        class TraceDeadLetter : IDeadLetterMessages
        {
            readonly ICollection<TransportMessage> collector;

            public TraceDeadLetter(ICollection<TransportMessage> collector)
            {
                this.collector = collector;
            }

            public Task DeadLetterAsync(TransportMessage message)
            {
                collector.Add(message);
                return Task.FromResult(0);
            }
        }
    }
}