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
    public class SendingMessagesLocal
    {
        Broker broker;
        Context context;
        HandlerRegistrySimulator registry;
        MessageUnit sender;

        [SetUp]
        public void SetUp()
        {
            context = new Context();
            registry = new HandlerRegistrySimulator(context);

            broker = new Broker();
            sender = new MessageUnit(new EndpointConfiguration().Endpoint("Sender").Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create("Receiver")))
                .Use(registry);

            broker.Register(sender);

            broker.Start();
        }

        [TearDown]
        public void TearDown()
        {
            broker.Stop();
        }

        [Test]
        public async Task WhenOneMessageSentLocal_InvokesSynchronousAndAsynchronousHandlers()
        {
            await sender.SendLocal(new Message
            {
                Bar = 42
            });

            context.FooAsyncHandlerCalled.Should().BeInvokedOnce();
            context.FooHandlerCalled.Should().BeInvokedOnce();
        }

        [Test]
        public async Task WhenMultipleMessageSentLocal_InvokesSynchronousAndAsynchronousHandlers()
        {
            await sender.SendLocal(new Message
            {
                Bar = 42
            });
            await sender.SendLocal(new Message
            {
                Bar = 43
            });

            context.FooAsyncHandlerCalled.Should().BeInvokedTwice();
            context.FooHandlerCalled.Should().BeInvokedTwice();
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
                        new MessageHandler(context));
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
                context.FooAsyncHandlerCalled += 1;
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
                context.FooHandlerCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            public int FooAsyncHandlerCalled { get; set; }
            public int FooHandlerCalled { get; set; }
        }
    }
}