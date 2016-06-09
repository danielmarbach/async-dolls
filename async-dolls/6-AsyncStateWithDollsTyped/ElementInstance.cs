using System;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    class ElementInstance
    {
        readonly ILinkElement instance;
        readonly IStepInvoker invoker;

        public ElementInstance(ILinkElement instance)
        {
            this.instance = instance;
            invoker = CreateInvoker(instance);
        }

        static IStepInvoker CreateInvoker(ILinkElement step)
        {
            var behaviorInterface = step.GetType().GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ILinkElement<,>));
            var invokerType = typeof(StepInvoker<,>).MakeGenericType(behaviorInterface.GetGenericArguments());
            return (IStepInvoker)Activator.CreateInstance(invokerType);
        }

        public Task Invoke(Context context, Func<Context, Task> next)
        {
            return invoker.Invoke(instance, context, next);
        }
    }
}