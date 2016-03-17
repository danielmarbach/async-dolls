using System;
using System.Threading.Tasks;

namespace AsyncDolls.YourDolls
{
    public interface IIncomingStep
    {
        Task Invoke(TransportMessage message, Func<Task> next);
    }
}