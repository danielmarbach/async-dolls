using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsPartial
{
    public abstract class StepConnector<TInContext, TOutContext> : IIncomingStep<TInContext, TOutContext>
        where TInContext : Context
        where TOutContext : Context
    {
        public abstract Task Invoke(TInContext context, Func<TOutContext, Task> next);
    }
}