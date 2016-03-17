using System;
using System.Threading.Tasks;

namespace AsyncDolls.Dequeuing
{
    class NoOpDequeStrategy : IDequeueStrategy
    {
        public Task StartAsync(EndpointConfiguration.ReadOnly configuration, Func<TransportMessage, Task> onMessage)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }
    }
}