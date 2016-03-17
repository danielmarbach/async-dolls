using System;
using System.Threading.Tasks;

namespace AsyncDollsSimple.Dequeuing
{
    public interface IIncomingStep
    {
        Task Invoke(TransportMessage message, Func<Task> next);
    }
}