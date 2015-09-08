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
        public async Task WhenOneMessagePublished_InvokesSynchronousAndAsynchronousHandlers()
        {
            await publisher.Publish(new Event
            {
                Bar = 42
            });

            context.AsyncHandlerCalled.Should().BeInvokedOnce();
            context.HandlerCalled.Should().BeInvokedOnce();
        }

        [Test]
        public async Task WhenMultipeMessagePublished_InvokesSynchronousAndAsynchronousHandlers()
        {
            await publisher.Publish(new Event
            {
                Bar = 42
            });
            await publisher.Publish(new Event
            {
                Bar = 43
            });

            context.AsyncHandlerCalled.Should().BeInvokedTwice();
            context.HandlerCalled.Should().BeInvokedTwice();
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
                if (messageType == typeof(Event))
                {
                    return this.ConsumeWith(
                        new AsyncMessageHandler(context),
                        new MessageHandler(context));
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncMessageHandler : IHandleMessageAsync<Event>
        {
            readonly Context context;

            public AsyncMessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Event message, IBusForHandler bus)
            {
                context.AsyncHandlerCalled += 1;
                context.AsyncHandlerCaughtHeaders = bus.Headers(message);
                return Task.FromResult(0);
            }
        }

        public class MessageHandler : IHandleMessageAsync<Event>
        {
            readonly Context context;

            public MessageHandler(Context context)
            {
                this.context = context;
            }

            public Task Handle(Event message, IBusForHandler bus)
            {
                context.HandlerCalled += 1;
                context.HandlerCaughtHeaders = bus.Headers(message);
                return Task.FromResult(0);
            }
        }

        public class Event
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