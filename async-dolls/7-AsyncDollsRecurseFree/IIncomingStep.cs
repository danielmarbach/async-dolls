using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsRecurseFree
{
    public interface IIncomingStep
    {
        Task Invoke(IncomingContext context, Func<Task> next);
    }
}