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
    public class ReplyingToMessages
    {
        const string SenderEndpointName = "Sender";
        const string ReceiverEndpointName = "Receiver";
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
            sender = new MessageUnit(new EndpointConfiguration().Endpoint(SenderEndpointName).Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create(ReceiverEndpointName)))
                .Use(registry);

            receiver = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverEndpointName).Concurrency(1))
                .Use(new AlwaysRouteToDestination(Queue.Create(SenderEndpointName)))
                .Use(registry);

            broker.Register(sender)
                .Register(receiver);

            broker.Start();
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
            context.ReplyHandlerCalls.Should().BeInvoked(3);
            context.HeaderValue.Should().Be("Value");
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
            await sender.Send(new Message
            {
                Bar = 44
            });
            await sender.Send(new Message
            {
                Bar = 45
            });

            context.FirstHandlerCalls.Should().BeInvoked(4);
            context.SecondHandlerCalls.Should().BeInvoked(4);
            context.ReplyHandlerCalls.Should().BeInvoked(12);
            context.HeaderValue.Should().Be("Value");
        }

        [TearDown]
        public void TearDown()
        {
            broker.Stop();
        }

        public class HandlerRegistrySimulator : HandlerRegistry
        {
            Context context;

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

                if (messageType == typeof(ReplyMessage))
                {
                    return this.ConsumeWith(new ReplyMessageHandler(context));
                }

                return this.ConsumeAll();
            }
        }

        public class ReplyMessageHandler : IHandleMessageAsync<ReplyMessage>
        {
            readonly Context context;

            public ReplyMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(ReplyMessage message, IBusForHandler bus)
            {
                context.ReplyHandlerCalls += 1;
                if (bus.Headers(message).ContainsKey("Key"))
                {
                    context.HeaderValue = bus.Headers(message)["Key"];
                }

                return Task.CompletedTask;
            }
        }

        public class FirstHandler : IHandleMessageAsync<Message>
        {
            readonly Context context;

            public FirstHandler(Context context)
            {
                this.context = context;
            }

            public async Task Handle(Message message, IBusForHandler bus)
            {
                context.FirstHandlerCalls += 1;
                await bus.Reply(new ReplyMessage
                {
                    Answer = "FirstHandler"
                });

                var options = new ReplyOptions();
                options.Headers.Add("Key", "Value");
                await bus.Reply(new ReplyMessage
                {
                    Answer = "FirstHandlerWithHeaders"
                }, options);
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
                return bus.Reply(new ReplyMessage
                {
                    Answer = "SecondHandler"
                });
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }

        public class ReplyMessage
        {
            public string Answer { get; set; }
        }

        public class Context
        {
            public int FirstHandlerCalls { get; set; }
            public int SecondHandlerCalls { get; set; }
            public int ReplyHandlerCalls { get; set; }
            public string HeaderValue { get; set; }
        }
    }
}