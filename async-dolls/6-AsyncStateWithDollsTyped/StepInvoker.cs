using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    class StepInvoker<TIn, TOut> : IStepInvoker
        where TOut : Context
        where TIn : Context
    {
        public Task Invoke(object behavior, Context context, Func<Context, Task> next)
        {
            return ((ILinkElement<TIn, TOut>)behavior).Invoke((TIn)context, next);
        }
    }
}