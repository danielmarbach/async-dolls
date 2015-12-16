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
    public class PublishingMessages
    {
        Broker broker;
        Context context;
        MessageUnit publisher;
        HandlerRegistrySimulator registry;
        MessageUnit subscriber;

        [SetUp]
        public void SetUp()
        {
            context = new Context();
            registry = new HandlerRegistrySimulator(context);

            broker = new Broker();
            Topic destination = Topic.Create("Subscriber");

            publisher = new MessageUnit(new EndpointConfiguration().Endpoint("Publisher").Concurrency(1))
                .Use(new AlwaysRouteToDestination(destination));
            subscriber = new MessageUnit(new EndpointConfiguration().Endpoint("Subscriber")
                .Concurrency(1)).Use(registry);

            broker.Register(publisher)
                .Register(subscriber, destination);

            broker.Start();
        }

        [TearDown]
        public void TearDown()
        {
            broker.Stop();
        }

        [Test]
        public async Task WhenOneMessagePublished_InvokesAllHandlers()
        {
            await publisher.Publish(new Event
            {
                Bar = 42
            });

            context.FirstHandlerCalls.Should().BeInvokedOnce();
            context.SecondHandlerCalls.Should().BeInvokedOnce();
        }

        [Test]
        public async Task WhenMultipeMessagePublished_InvokesAllHandlersForEachMessage()
        {
            await publisher.Publish(new Event
            {
                Bar = 42
            });
            await publisher.Publish(new Event
            {
                Bar = 43
            });

            context.FirstHandlerCalls.Should().BeInvokedTwice();
            context.SecondHandlerCalls.Should().BeInvokedTwice();
        }

        [Test]
        public async Task WhenPublishingMessagesWithCustomHeaders_HeadersCanBeReadOnSubscriberSide()
        {
            const string HeaderKey = "MyHeader";
            const string HeaderValue = "MyValue";

            var publishOptions = new PublishOptions();
            publishOptions.Headers.Add(HeaderKey, HeaderValue);

            await publisher.Publish(new Event
            {
                Bar = 42
            }, publishOptions);

            context.SecondHandlerCaughtHeaders.Should()
                .Contain(HeaderKey, HeaderValue);
            context.FirstcHandlerCaughtHeaders.Should()
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
                if (messageType == typeof(Event))
                {
                    return this.ConsumeWith(
                        new FirstHandler(context),
                        new SecondHandler(context));
                }

                return this.ConsumeAll();
            }
        }

        public class FirstHandler : IHandleMessageAsync<Event>
        {
            readonly Context context;

            public FirstHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Event message, IBusForHandler bus)
            {
                context.FirstHandlerCalls += 1;
                context.FirstcHandlerCaughtHeaders = bus.Headers(message);
                return Task.CompletedTask;
            }
        }

        public class SecondHandler : IHandleMessageAsync<Event>
        {
            readonly Context context;

            public SecondHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Event message, IBusForHandler bus)
            {
                context.SecondHandlerCalls += 1;
                context.SecondHandlerCaughtHeaders = bus.Headers(message);
                return Task.CompletedTask;
            }
        }

        public class Event
        {
            public int Bar { get; set; }
        }

        public class Context
        {
            public int FirstHandlerCalls { get; set; }
            public int SecondHandlerCalls { get; set; }
            public IDictionary<string, string> FirstcHandlerCaughtHeaders { get; set; }
            public IDictionary<string, string> SecondHandlerCaughtHeaders { get; set; }
        }
    }
}