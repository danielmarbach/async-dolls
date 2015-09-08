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

            context.AsyncReceiverOneCalled.Should().BeInvokedOnce();
            context.SyncReceiverOneCalled.Should().BeInvokedOnce();
            context.AsyncReceiverTwoCalled.Should().BeInvokedOnce();
            context.SyncReceiverTwoCalled.Should().BeInvokedOnce();
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

            context.AsyncReceiverOneCalled.Should().NotBeInvoked();
            context.SyncReceiverOneCalled.Should().NotBeInvoked();
            context.AsyncReceiverTwoCalled.Should().NotBeInvoked();
            context.SyncReceiverTwoCalled.Should().NotBeInvoked();

            context.AsyncReceiverThreeCalled.Should().BeInvokedOnce();
            context.SyncReceiverThreeCalled.Should().BeInvokedOnce();
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
                        new AsyncMessageHandlerReceiverOne(context),
                        new MessageHandlerReceiverOne(context));
                }

                if (messageType == typeof(MessageForReceiverTwo))
                {
                    return this.ConsumeWith(
                        new AsyncMessageHandlerReceiverTwo(context),
                        new MessageHandlerReceiverTwo(context));
                }

                if (messageType == typeof(MessageForReceiverThree))
                {
                    return this.ConsumeWith(
                        new AsyncMessageHandlerReceiverThree(context),
                        new MessageHandlerReceiverThree(context));
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncMessageHandlerReceiverOne : IHandleMessageAsync<MessageForReceiverOne>
        {
            readonly Context context;

            public AsyncMessageHandlerReceiverOne(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverOne message, IBusForHandler bus)
            {
                context.AsyncReceiverOneCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class MessageHandlerReceiverOne : IHandleMessageAsync<MessageForReceiverOne>
        {
            readonly Context context;

            public MessageHandlerReceiverOne(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverOne message, IBusForHandler bus)
            {
                context.SyncReceiverOneCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class AsyncMessageHandlerReceiverTwo : IHandleMessageAsync<MessageForReceiverTwo>
        {
            readonly Context context;

            public AsyncMessageHandlerReceiverTwo(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverTwo message, IBusForHandler bus)
            {
                context.AsyncReceiverTwoCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class MessageHandlerReceiverTwo : IHandleMessageAsync<MessageForReceiverTwo>
        {
            readonly Context context;

            public MessageHandlerReceiverTwo(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverTwo message, IBusForHandler bus)
            {
                context.SyncReceiverTwoCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class AsyncMessageHandlerReceiverThree : IHandleMessageAsync<MessageForReceiverThree>
        {
            readonly Context context;

            public AsyncMessageHandlerReceiverThree(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverThree message, IBusForHandler bus)
            {
                context.AsyncReceiverThreeCalled += 1;
                return Task.FromResult(0);
            }
        }

        public class MessageHandlerReceiverThree : IHandleMessageAsync<MessageForReceiverThree>
        {
            readonly Context context;

            public MessageHandlerReceiverThree(Context context)
            {
                this.context = context;
            }

            public Task Handle(MessageForReceiverThree message, IBusForHandler bus)
            {
                context.SyncReceiverThreeCalled += 1;
                return Task.FromResult(0);
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
            public int AsyncReceiverOneCalled { get; set; }
            public int SyncReceiverOneCalled { get; set; }
            public int AsyncReceiverTwoCalled { get; set; }
            public int SyncReceiverTwoCalled { get; set; }
            public int AsyncReceiverThreeCalled { get; set; }
            public int SyncReceiverThreeCalled { get; set; }
        }
    }
}