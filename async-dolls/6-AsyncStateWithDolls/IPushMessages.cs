using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDolls
{
    public interface IPushMessages
    {
        Task StartAsync(Func<TransportMessage, Task> onMessage);
        Task StopAsync();
    }
}