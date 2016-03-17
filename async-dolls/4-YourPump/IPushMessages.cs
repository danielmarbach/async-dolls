using System;
using System.Threading.Tasks;

namespace AsyncDolls.YourPump
{
    public interface IPushMessages
    {
        Task StartAsync(Func<TransportMessage, Task> onMessage);
        Task StopAsync();
    }
}