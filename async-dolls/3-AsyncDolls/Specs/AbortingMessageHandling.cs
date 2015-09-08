namespace AsyncDolls.Specs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using Pipeline;
    using Testing;

    [TestFixture]
    public class AbortingMessageHandling
    {
        Broker broker;
        Context context;
        MessageUnit receiver;
        HandlerRegistrySimulator registry;
        MessageUnit sender;

        [SetUp]
        public void SetUp()
        {
            context = new Context();
            registry = new HandlerRegistrySimulator(context);

            broker = new Broker();
            sender = new MessageUnit(new EndpointConfiguration().Endpoint("Sender").Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create("Receiver")));
            receiver = new MessageUnit(new EndpointConfiguration().Endpoint("Receiver")
                .Concurrency(1)).Use(registry);

            broker.Register(sender)
                .Register(receiver);

            broker.Start();
        }

        [TearDown]
        public void TearDown()
        {
            broker.Stop();
        }

        [Test]
        public async Task WhenPipelineAbortedAsync_ShouldNotContinueToSyncHandler()
        {
            await sender.Send(new Message
            {
                AbortAsync = true,
                AbortSync = false,
                Bar = 42
            });

            context.AsyncHandlerCalled.Should().BeInvokedOnce();
            context.HandlerCalled.Should().NotBeInvoked();
            context.LastHandlerCalled.Should().NotBeInvoked();
        }

        [Test]
        public async Task WhenPipelineAbortedSync_ShouldNotContinueToLastHandler()
        {
            await sender.Send(new Message
            {
                AbortAsync = false,
                AbortSync = true,
                Bar = 42
            });

            context.AsyncHandlerCalled.Should().BeInvokedOnce();
            context.HandlerCalled.Should().BeInvokedOnce();
            context.LastHandlerCalled.Should().NotBeInvoked();
        }

        [Test]
        public async Task WhenPipelineNotAborted_ShouldExecuteAllHandler()
        {
            await sender.Send(new Message
            {
                AbortAsync = false,
                AbortSync = false,
                Bar = 42
            });

            context.AsyncHandlerCalled.Should().BeInvokedOnce();
            context.HandlerCalled.Should().BeInvokedOnce();
            context.LastHandlerCalled.Should().BeInvokedOnce();
        }

        public class HandlerRegistrySimulator : HandlerRegistry
        {
            readonly Context context;

            public HandlerRegistrySimulator(Context context)
            {
                this.context = context;
            }

            public override IReadOnlyCollection<object> GetHandlers(Type messageType)
            {
                if (messageType == typeof(Message))
                {
                    return this.ConsumeWith(
                        new AsyncMessageHandler(context),
                        new LastHandler(context));
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncMessageHandler : IHandleMessageAsync<Message>
        {
            readonly Context context;

            public AsyncMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Message message, IBusForHandler bus)
            {
                context.AsyncHandlerCalled += 1;

                if (message.AbortAsync)
                {
                    bus.DoNotContinueDispatchingCurrentMessageToHandlers();
                }

                return Task.FromResult(0);
            }
        }

        public class MessageHandler : IHandleMessageAsync<Message>
        {
            readonly Context context;

            public MessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Message message, IBusForHandler bus)
            {
                context.HandlerCalled += 1;

                if (message.AbortSync)
                {
                    bus.DoNotContinueDispatchingCurrentMessageToHandlers();
                }

                return Task.FromResult(0);
            }
        }

        public class LastHandler : IHandleMessageAsync<Message>
        {
            readonly Context context;

            public LastHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Message message, IBusForHandler bus)
            {
                context.LastHandlerCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class Message
        {
            public bool AbortAsync { get; set; }
            public bool AbortSync { get; set; }
            public int Bar { get; set; }
        }

        public class Context
        {
            public int AsyncHandlerCalled { get; set; }
            public int HandlerCalled { get; set; }
            public int LastHandlerCalled { get; set; }
        }
    }
}