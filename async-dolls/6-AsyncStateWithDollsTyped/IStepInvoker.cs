using System;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    interface IStepInvoker
    {
        Task Invoke(object behavior, Context context, Func<Context, Task> next);
    }
}