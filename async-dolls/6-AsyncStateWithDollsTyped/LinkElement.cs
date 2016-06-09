using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    public abstract class LinkElement<TContext> : ILinkElement<TContext, TContext>
        where TContext : Context
    {
        public Task Invoke(TContext context, Func<TContext, Task> next)
        {
            return Invoke(context, () => next(context));
        }

        public abstract Task Invoke(TContext context, Func<Task> next);
    }
}