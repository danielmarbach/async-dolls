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
        public async Task WhenOneMessageSentLocal_InvokesAllHandlers()
        {
            await sender.SendLocal(new Message
            {
                Bar = 42
            });

            context.FirstHandlerCalls.Should().BeInvokedOnce();
            context.SecondHandlerCalls.Should().BeInvokedOnce();
        }

        [Test]
        public async Task WhenMultipleMessageSentLocal_InvokesAllHandlersForEachMessage()
        {
            await sender.SendLocal(new Message
            {
                Bar = 42
            });
            await sender.SendLocal(new Message
            {
                Bar = 43
            });

            context.FirstHandlerCalls.Should().BeInvokedTwice();
            context.SecondHandlerCalls.Should().BeInvokedTwice();
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
        }
    }
}