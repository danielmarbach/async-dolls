using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls.YourDolls
{
    public class Chain
    {
        readonly List<ILinkElement> linkElements;

        public Chain(IEnumerable<ILinkElement> steps)
        {
            linkElements = new List<ILinkElement>(steps);
        }

        public Task Invoke(TransportMessage transportMessage)
        {
            return InnerInvoke(transportMessage);
        }

        Task InnerInvoke(TransportMessage transportMessage, int currentIndex = 0)
        {
            if (currentIndex == linkElements.Count)
            {
                return Task.CompletedTask;
            }

            ILinkElement step = linkElements[currentIndex];

            return step.Invoke(transportMessage, () => InnerInvoke(transportMessage, currentIndex + 1));
        }
    }
}