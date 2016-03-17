using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncDolls.YourPump
{
    [TestFixture]
    public class SimpleSpec
    {
        private AsyncCountdownEvent countdown;

        [SetUp]
        public void SetUp()
        {
            countdown = new AsyncCountdownEvent(3);
        }

        [Test]
        public async Task Do()
        {
            var messages = new ConcurrentQueue<TransportMessage>();
            messages.Enqueue(new TransportMessage());
            messages.Enqueue(new TransportMessage());
            messages.Enqueue(new TransportMessage());

            var strategy = new PushMessages(messages, maxConcurrency: 1);

            await strategy.StartAsync(HandleMessage);

            await countdown.WaitAsync();

            await strategy.StopAsync();
        }

        public Task HandleMessage(TransportMessage message)
        {
            message.Id.Output();
            countdown.Signal();
            return Task.CompletedTask;
        }
    }
}