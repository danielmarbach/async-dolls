using System;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsPartial
{
    class ElementInstance
    {
        readonly ILinkElement instance;
        readonly IElementInvoker invoker;

        public ElementInstance(ILinkElement instance)
        {
            this.instance = instance;
            invoker = CreateInvoker(instance);

            // You would do it smarter
            IsBefore = instance.GetType().BaseType.Name.StartsWith("Before");
            IsAfter = instance.GetType().BaseType.Name.StartsWith("After");

            if (IsAfter || IsBefore)
                return;

            IsSurround = true;
        }

        public bool IsBefore { get; }
        public bool IsAfter { get; }
        public bool IsSurround { get; }

        static IElementInvoker CreateInvoker(ILinkElement step)
        {
            var behaviorInterface = step.GetType().GetInterfaces().First(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ILinkElement<,>));
            var invokerType = typeof(ElementInvoker<,>).MakeGenericType(behaviorInterface.GetGenericArguments());
            return (IElementInvoker)Activator.CreateInstance(invokerType);
        }

        public Task Invoke(Context context, Func<Context, Task> next)
        {
            return invoker.Invoke(instance, context, next);
        }
    }
}