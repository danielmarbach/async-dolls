using System;
using System.Threading.Tasks;

namespace AsyncDollsSimple.Dequeuing
{
    public interface IDequeueStrategy
    {
        Task StartAsync(Func<TransportMessage, Task> onMessage);
        Task StopAsync();
    }
}