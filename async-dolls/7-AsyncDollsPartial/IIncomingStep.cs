using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsPartial
{
    public interface IIncomingStep { }

    public interface IIncomingStep<in TInContext, out TOutContext> : IIncomingStep
        where TInContext : Context
        where TOutContext : Context
    {
        Task Invoke(TInContext context, Func<TOutContext, Task> next);
    }
}