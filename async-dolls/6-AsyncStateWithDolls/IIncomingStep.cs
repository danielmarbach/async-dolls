using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDolls
{
    public interface IIncomingStep
    {
        Task Invoke(IncomingContext context, Func<Task> next);
    }
}