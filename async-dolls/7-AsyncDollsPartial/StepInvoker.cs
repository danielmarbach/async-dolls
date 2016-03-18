using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsPartial
{
    class StepInvoker<TIn, TOut> : IStepInvoker
        where TOut : Context
        where TIn : Context
    {
        public Task Invoke(object behavior, Context context, Func<Context, Task> next)
        {
            return ((IIncomingStep<TIn, TOut>)behavior).Invoke((TIn)context, next as Func<TOut, Task>);
        }
    }
}