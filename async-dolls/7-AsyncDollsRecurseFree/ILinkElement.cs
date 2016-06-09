using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsRecurseFree
{
    public interface ILinkElement
    {
        Task Invoke(IncomingContext context, Func<Task> next);
    }
}