using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    public interface ILinkElement { }

    public interface ILinkElement<in TInContext, out TOutContext> : ILinkElement
        where TInContext : Context
        where TOutContext : Context
    {
        Task Invoke(TInContext context, Func<TOutContext, Task> next);
    }
}