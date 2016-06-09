using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsInDepth
{
    public interface ILinkElement
    {
        Task Invoke(IncomingContext context, Func<Task> next);
    }
}