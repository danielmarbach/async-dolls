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
        public async Task WhenOneMessageSent_InvokesAllHandlers()
        {
            await sender.Send(new Message
            {
                Bar = 42
            });

            context.FirstHandlerCalls.Should().BeInvokedOnce();
            context.SecondHandlerCalls.Should().BeInvokedOnce();
        }

        [Test]
        public async Task WhenMultipeMessageSent_InvokesAllHandlersForEachMessage()
        {
            await sender.Send(new Message
            {
                Bar = 42
            });
            await sender.Send(new Message
            {
                Bar = 43
            });

            context.FirstHandlerCalls.Should().BeInvokedTwice();
            context.SecondHandlerCalls.Should().BeInvokedTwice();
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

            context.SecondHandlerCaughtHeaders.Should()
                .Contain(HeaderKey, HeaderValue);
            context.FirstHandlerCaughtHeaders.Should()
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
                        new FirstHandler(context),
                        new SecondHandler(context));
                }

                return this.ConsumeAll();
            }
        }

        public class FirstHandler : IHandleMessageAsync<Message>
        {
            readonly Context context;

            public FirstHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Message message, IBusForHandler bus)
            {
                context.FirstHandlerCalls += 1;
                context.FirstHandlerCaughtHeaders = bus.Headers(message);
                return Task.FromResult(0);
            }
        }

        public class SecondHandler : IHandleMessageAsync<Message>
        {
            readonly Context context;

            public SecondHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Message message, IBusForHandler bus)
            {
                context.SecondHandlerCalls += 1;
                context.SecondHandlerCaughtHeaders = bus.Headers(message);
                return Task.FromResult(0);
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            public int FirstHandlerCalls { get; set; }
            public int SecondHandlerCalls { get; set; }
            public IDictionary<string, string> FirstHandlerCaughtHeaders { get; set; }
            public IDictionary<string, string> SecondHandlerCaughtHeaders { get; set; }
        }
    }
}