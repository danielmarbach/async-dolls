using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsRecurseFree
{
    public interface IPushMessages
    {
        Task StartAsync(Func<TransportMessage, Task> onMessage);
        Task StopAsync();
    }
}