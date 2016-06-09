using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    public class Chain
    {
        readonly List<ElementInstance> executingElements;

        public Chain(IEnumerable<ILinkElement> elements)
        {
            executingElements = elements.Select(s => new ElementInstance(s)).ToList();
        }

        public Task Invoke(Context context)
        {
            return InnerInvoke(context);
        }

        Task InnerInvoke(Context context, int currentIndex = 0)
        {
            if (currentIndex == executingElements.Count)
            {
                return Task.CompletedTask;
            }

            ElementInstance element = executingElements[currentIndex];

            return element.Invoke(context, ctx => InnerInvoke(ctx, currentIndex + 1));
        }
    }
}