namespace AsyncDolls.Specs
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using Pipeline;
    using Pipeline.Outgoing;
    using Testing;

    [TestFixture]
    public class RoutingMessages
    {
        const string ReceiverOneEndpointName = "Receiver1";
        const string ReceiverTwoEndpointName = "Receiver2";
        const string ReceiverThreeEndpointName = "Receiver3";
        Broker broker;
        Context context;
        MessageUnit receiverOne;
        MessageUnit receiverThree;
        MessageUnit receiverTwo;
        HandlerRegistrySimulator registry;
        Router router;
        MessageUnit sender;

        [SetUp]
        public void SetUp()
        {
            context = new Context();
            registry = new HandlerRegistrySimulator(context);
            router = new Router();

            broker = new Broker();
            sender = new MessageUnit(new EndpointConfiguration().Endpoint("Sender").Concurrency(1))
                .Use(router);
            receiverOne = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverOneEndpointName)
                .Concurrency(1)).Use(registry);
            receiverTwo = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverTwoEndpointName)
                .Concurrency(1)).Use(registry);
            receiverThree = new MessageUnit(new EndpointConfiguration().Endpoint(ReceiverThreeEndpointName)
                .Concurrency(1)).Use(registry);

            broker.Register(sender)
                .Register(receiverOne)
                .Register(receiverTwo)
                .Register(receiverThree);

            broker.Start();
        }

        [TearDown]
        public void TearDown()
        {
            broker.Stop();
        }

        [Test]
        public async Task WhenSendingMessages_MessagesAreRoutedAccordingToTheMessageRouter()
        {
            await sender.Send(new MessageForReceiverOne
            {
                Bar = 42
            });
            await sender.Send(new MessageForReceiverTwo
            {
                Bar = 42
            });

            context.FirstHandlerReceiverOneCalls.Should().BeInvokedOnce();
            context.SecondHandlerReceiverOneCalls.Should().BeInvokedOnce();
            context.FirstHandlerReceiverTwoCalls.Should().BeInvokedOnce();
            context.SecondHandlerReceiverTwoCalls.Should().BeInvokedOnce();
        }

        [Test]
        public async Task WhenSendingMessages_WithSpecificSendDestination_MessagesAreRoutedByUserInput()
        {
            var sendOptions = new SendOptions
            {
                Queue = Queue.Create(ReceiverThreeEndpointName)
            };

            await sender.Send(new MessageForReceiverThree
            {
                Bar = 42
            }, sendOptions);

            context.FirstHandlerReceiverOneCalls.Should().NotBeInvoked();
            context.SecondHandlerReceiverOneCalls.Should().NotBeInvoked();
            context.FirstHandlerReceiverTwoCalls.Should().NotBeInvoked();
            context.SecondHandlerReceiverTwoCalls.Should().NotBeInvoked();

            context.FirstHandlerReceiverThreeCalls.Should().BeInvokedOnce();
            context.SecondHandlerReceiverThreeCalls.Should().BeInvokedOnce();
        }

        class Router : IMessageRouter
        {
            public IReadOnlyCollection<Address> GetDestinationFor(Type messageType)
            {
                if (messageType == typeof(MessageForReceiverOne))
                {
                    return this.To(Queue.Create(ReceiverOneEndpointName));
                }

                if (messageType == typeof(MessageForReceiverTwo))
                {
                    return this.To(Queue.Create(ReceiverTwoEndpointName));
                }

                return this.NoDestination();
            }
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
                if (messageType == typeof(MessageForReceiverOne))
                {
                    return this.ConsumeWith(
                        new FirstHandlerReceiverOne(context),
                        new SecondHandlerReceiverOne(context));
                }

                if (messageType == typeof(MessageForReceiverTwo))
                {
                    return this.ConsumeWith(
                        new FirstHandlerReceiverTwo(context),
                        new SecondHandlerReceiverTwo(context));
                }

                if (messageType == typeof(MessageForReceiverThree))
                {
                    return this.ConsumeWith(
                        new FirstHandlerReceiverThree(context),
                        new SecondHandlerReceiverThree(context));
                }

                return this.ConsumeAll();
            }
        }

        public class FirstHandlerReceiverOne : IHandleMessageAsync<MessageForReceiverOne>
        {
            readonly Context context;

            public FirstHandlerReceiverOne(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverOne message, IBusForHandler bus)
            {
                context.FirstHandlerReceiverOneCalls += 1;
                return Task.CompletedTask;
            }
        }

        public class SecondHandlerReceiverOne : IHandleMessageAsync<MessageForReceiverOne>
        {
            readonly Context context;

            public SecondHandlerReceiverOne(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverOne message, IBusForHandler bus)
            {
                context.SecondHandlerReceiverOneCalls += 1;
                return Task.CompletedTask;
            }
        }

        public class FirstHandlerReceiverTwo : IHandleMessageAsync<MessageForReceiverTwo>
        {
            readonly Context context;

            public FirstHandlerReceiverTwo(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverTwo message, IBusForHandler bus)
            {
                context.FirstHandlerReceiverTwoCalls += 1;
                return Task.CompletedTask;
            }
        }

        public class SecondHandlerReceiverTwo : IHandleMessageAsync<MessageForReceiverTwo>
        {
            readonly Context context;

            public SecondHandlerReceiverTwo(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverTwo message, IBusForHandler bus)
            {
                context.SecondHandlerReceiverTwoCalls += 1;
                return Task.CompletedTask;
            }
        }

        public class FirstHandlerReceiverThree : IHandleMessageAsync<MessageForReceiverThree>
        {
            readonly Context context;

            public FirstHandlerReceiverThree(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverThree message, IBusForHandler bus)
            {
                context.FirstHandlerReceiverThreeCalls += 1;
                return Task.CompletedTask;
            }
        }

        public class SecondHandlerReceiverThree : IHandleMessageAsync<MessageForReceiverThree>
        {
            readonly Context context;

            public SecondHandlerReceiverThree(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverThree message, IBusForHandler bus)
            {
                context.SecondHandlerReceiverThreeCalls += 1;
                return Task.CompletedTask;
            }
        }

        public class MessageForReceiverOne
        {
            public int Bar { get; set; }
        }

        public class MessageForReceiverTwo
        {
            public int Bar { get; set; }
        }

        public class MessageForReceiverThree
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            public int FirstHandlerReceiverOneCalls { get; set; }
            public int SecondHandlerReceiverOneCalls { get; set; }
            public int FirstHandlerReceiverTwoCalls { get; set; }
            public int SecondHandlerReceiverTwoCalls { get; set; }
            public int FirstHandlerReceiverThreeCalls { get; set; }
            public int SecondHandlerReceiverThreeCalls { get; set; }
        }
    }
}