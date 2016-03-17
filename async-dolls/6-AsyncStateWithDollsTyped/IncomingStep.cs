using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    public abstract class IncomingStep<TContext> : IIncomingStep<TContext, TContext> 
        where TContext : Context
    {
        public Task Invoke(TContext context, Func<TContext, Task> next)
        {
            return Invoke(context, () => next(context));
        }

        public abstract Task Invoke(TContext context, Func<Task> next);
    }
}