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
    public class SendingMessages
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
        public async Task WhenOneMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await sender.Send(new Message
            {
                Bar = 42
            });

            context.AsyncHandlerCalled.Should().BeInvokedOnce();
            context.HandlerCalled.Should().BeInvokedOnce();
        }

        [Test]
        public async Task WhenMultipeMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await sender.Send(new Message
            {
                Bar = 42
            });
            await sender.Send(new Message
            {
                Bar = 43
            });

            context.AsyncHandlerCalled.Should().BeInvokedTwice();
            context.HandlerCalled.Should().BeInvokedTwice();
        }

        [Test]
        public async Task WhenSendingMessagesWithCustomHeaders_HeadersCanBeReadOnReceiverSide()
        {
            const string HeaderKey = "MyHeader";
            const string HeaderValue = "MyValue";

            var sendOptions = new SendOptions();
            sendOptions.Headers.Add(HeaderKey, HeaderValue);

            await sender.Send(new Message
            {
                Bar = 42
            }, sendOptions);

            context.HandlerCaughtHeaders.Should()
                .Contain(HeaderKey, HeaderValue);
            context.AsyncHandlerCaughtHeaders.Should()
                .Contain(HeaderKey, HeaderValue);
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
                context.AsyncHandlerCalled += 1;
                context.AsyncHandlerCaughtHeaders = bus.Headers(message);
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
                context.HandlerCaughtHeaders = bus.Headers(message);
                return Task.FromResult(0);
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            public int AsyncHandlerCalled { get; set; }
            public int HandlerCalled { get; set; }
            public IDictionary<string, string> AsyncHandlerCaughtHeaders { get; set; }
            public IDictionary<string, string> HandlerCaughtHeaders { get; set; }
        }
    }
}