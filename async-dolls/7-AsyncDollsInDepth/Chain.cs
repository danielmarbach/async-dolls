using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsInDepth
{
    public class Chain
    {
        readonly List<ILinkElement> executingElements;

        public Chain(IEnumerable<ILinkElement> elements)
        {
            executingElements = new List<ILinkElement>(elements);
        }

        public Task Invoke(IncomingContext context)
        {
            return InnerInvoke(context);
        }

        Task InnerInvoke(IncomingContext context, int currentIndex = 0)
        {
            if (currentIndex == executingElements.Count)
            {
                return Task.CompletedTask;
            }

            ILinkElement element = executingElements[currentIndex];

            return element.Invoke(context, () => InnerInvoke(context, currentIndex + 1));
        }
    }
}