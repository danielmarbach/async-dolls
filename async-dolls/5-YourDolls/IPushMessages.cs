using System;
using System.Threading.Tasks;

namespace AsyncDolls.YourDolls
{
    public interface IPushMessages
    {
        Task StartAsync(Func<TransportMessage, Task> onMessage);
        Task StopAsync();
    }
}