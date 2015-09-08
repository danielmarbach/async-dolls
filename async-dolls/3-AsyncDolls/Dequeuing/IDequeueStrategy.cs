using System;
using System.Threading.Tasks;

namespace AsyncDolls.Dequeuing
{
    public interface IDequeueStrategy
    {
        Task StartAsync(EndpointConfiguration.ReadOnly configuration, Func<TransportMessage, Task> onMessage);
        Task StopAsync();
    }
}