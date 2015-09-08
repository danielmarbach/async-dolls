namespace AsyncDolls.Specs
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using FluentAssertions;
    using NUnit.Framework;
    using Pipeline;
    using Testing;

    [TestFixture]
    public class DeadLetterMessages
    {
        Broker broker;
        MessageUnit receiver;
        HandlerRegistrySimulator registry;
        MessageUnit sender;

        [SetUp]
        public void SetUp()
        {
            registry = new HandlerRegistrySimulator();

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
        public void WhenMessageSentWithBodyWhichCannotBeDeserialized_MessageIsDeadlettered()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("{ ; }");
            writer.Flush();
            stream.Position = 0;

            var tm = new DeadLetterTransportMessage
            {
                MessageType = typeof(Message).AssemblyQualifiedName
            };
            tm.SetBody(stream);

            Func<Task> action = () => receiver.HandOver(tm);

            action.ShouldThrow<SerializationException>();
            tm.DeadLetterHeaders.Should().NotBeEmpty();
        }

        [Test]
        public void WhenMessageReachesMaximumNumberOfRetries_MessageIsDeadlettered()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("{ Bar: 1 }");
            writer.Flush();
            stream.Position = 0;

            var tm = new DeadLetterTransportMessage
            {
                MessageType = typeof(Message).AssemblyQualifiedName
            };
            tm.SetBody(stream);

            Func<Task> action = () => receiver.HandOver(tm);

            action.ShouldThrow<InvalidOperationException>();
            tm.DeadLetterHeaders.Should().NotBeEmpty();
        }

        public class DeadLetterTransportMessage : TransportMessage
        {
            public IDictionary<string, object> DeadLetterHeaders { get; private set; }

            public override int DeliveryCount
            {
                get { return 10; }
            }

            //protected override Task DeadLetterAsyncInternal(IDictionary<string, object> deadLetterHeaders)
            //{
            //    DeadLetterHeaders = deadLetterHeaders;

            //    return Task.FromResult(0);
            //}
        }

        public class HandlerRegistrySimulator : HandlerRegistry
        {
            public override IReadOnlyCollection<object> GetHandlers(Type messageType)
            {
                if (messageType == typeof(Message))
                {
                    return this.ConsumeWith(new AsyncHandlerWhichFailsAllTheTime());
                }

                return this.ConsumeAll();
            }
        }

        public class AsyncHandlerWhichFailsAllTheTime : IHandleMessageAsync<Message>
        {
            public Task Handle(Message message, IBusForHandler bus)
            {
                throw new InvalidOperationException();
            }
        }

        public class Message
        {
            public int Bar { get; set; }
        }
    }
}