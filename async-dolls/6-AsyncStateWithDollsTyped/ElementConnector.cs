using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    public abstract class ElementConnector<TInContext, TOutContext> : ILinkElement<TInContext, TOutContext>
        where TInContext : Context
        where TOutContext : Context
    {
        public abstract Task Invoke(TInContext context, Func<TOutContext, Task> next);
    }
}