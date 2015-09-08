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
        public async Task WhenOneMessageSent_InvokesSynchronousAndAsynchronousHandlers()
        {
            await sender.Send(new Message
            {
                Bar = 42
            });

            context.AsyncHandlerCalls.Should().BeInvokedOnce();
            context.HandlerCalls.Should().BeInvokedOnce();
            context.ReplyHandlerCalls.Should().BeInvoked(3);
            context.HeaderValue.Should().Be("Value");
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
            await sender.Send(new Message
            {
                Bar = 44
            });
            await sender.Send(new Message
            {
                Bar = 45
            });

            context.AsyncHandlerCalls.Should().BeInvoked(4);
            context.HandlerCalls.Should().BeInvoked(4);
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
                        new AsyncMessageHandler(context),
                        new MessageHandler(context));
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

                return Task.FromResult(0);
            }
        }

        public class AsyncMessageHandler : IHandleMessageAsync<Message>
        {
            readonly Context context;

            public AsyncMessageHandler(Context context)
            {
                this.context = context;
            }

            public async Task Handle(Message message, IBusForHandler bus)
            {
                context.AsyncHandlerCalls += 1;
                await bus.Reply(new ReplyMessage
                {
                    Answer = "AsyncMessageHandler"
                });

                var options = new ReplyOptions();
                options.Headers.Add("Key", "Value");
                await bus.Reply(new ReplyMessage
                {
                    Answer = "AsyncMessageHandlerWithHeaders"
                }, options);
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
                context.HandlerCalls += 1;
                return bus.Reply(new ReplyMessage
                {
                    Answer = "MessageHandler"
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
            public int AsyncHandlerCalls { get; set; }
            public int HandlerCalls { get; set; }
            public int ReplyHandlerCalls { get; set; }
            public string HeaderValue { get; set; }
        }
    }
}