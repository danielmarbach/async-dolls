using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsInDepth
{
    public interface IIncomingStep
    {
        Task Invoke(IncomingContext context, Func<Task> next);
    }
}