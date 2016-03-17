using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AsyncDolls.YourPump
{
    [TestFixture]
    public class Spec
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

        public async Task HandleMessage(TransportMessage message)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            message.Id.Output();
            countdown.Signal();
        }
    }
}