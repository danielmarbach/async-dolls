using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDolls
{
    public interface ILinkElement
    {
        Task Invoke(IncomingContext context, Func<Task> next);
    }
}