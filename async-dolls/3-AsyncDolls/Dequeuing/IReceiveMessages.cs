using System;
using System.Threading.Tasks;

namespace AsyncDolls.Dequeuing
{
    public interface IReceiveMessages
    {
        Task<AsyncClosable> StartAsync(EndpointConfiguration.ReadOnly configuration, Func<TransportMessage, Task> onMessage);
    }
}