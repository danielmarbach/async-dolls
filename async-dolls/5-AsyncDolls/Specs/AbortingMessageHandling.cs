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
        public async Task WhenPipelineAbortedFirstHandler_OnlyInvokesFirstHandler()
        {
            await sender.Send(new Message
            {
                AbortFirstHandler = true,
                AbortSecondHandler = false,
                Bar = 42
            });

            context.FirstHandlerCalls.Should().BeInvokedOnce();
            context.SecondHandlerCalls.Should().NotBeInvoked();
            context.LastHandlerCalls.Should().NotBeInvoked();
        }

        [Test]
        public async Task WhenPipelineAbortedSecondHandler_DoesntInvokeLastHandler()
        {
            await sender.Send(new Message
            {
                AbortFirstHandler = false,
                AbortSecondHandler = true,
                Bar = 42
            });

            context.FirstHandlerCalls.Should().BeInvokedOnce();
            context.SecondHandlerCalls.Should().BeInvokedOnce();
            context.LastHandlerCalls.Should().NotBeInvoked();
        }

        [Test]
        public async Task WhenPipelineNotAborted_InvokesAllHandlers()
        {
            await sender.Send(new Message
            {
                AbortFirstHandler = false,
                AbortSecondHandler = false,
                Bar = 42
            });

            context.FirstHandlerCalls.Should().BeInvokedOnce();
            context.SecondHandlerCalls.Should().BeInvokedOnce();
            context.LastHandlerCalls.Should().BeInvokedOnce();
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
                        new SecondHandler(context),
                        new LastHandler(context));
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

                if (message.AbortFirstHandler)
                {
                    bus.DoNotContinueDispatchingCurrentMessageToHandlers();
                }

                return Task.CompletedTask;
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

                if (message.AbortSecondHandler)
                {
                    bus.DoNotContinueDispatchingCurrentMessageToHandlers();
                }

                return Task.CompletedTask;
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
                context.LastHandlerCalls += 1;
                return Task.CompletedTask;
            }
        }

        public class Message
        {
            public bool AbortFirstHandler { get; set; }
            public bool AbortSecondHandler { get; set; }
            public int Bar { get; set; }
        }

        public class Context
        {
            public int FirstHandlerCalls { get; set; }
            public int SecondHandlerCalls { get; set; }
            public int LastHandlerCalls { get; set; }
        }
    }
}