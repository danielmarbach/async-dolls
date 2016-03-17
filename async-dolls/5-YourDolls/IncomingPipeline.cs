using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls.YourDolls
{
    public class IncomingPipeline
    {
        readonly List<IIncomingStep> executingSteps;

        public IncomingPipeline(IEnumerable<IIncomingStep> steps)
        {
            executingSteps = new List<IIncomingStep>(steps);
        }

        public Task Invoke(TransportMessage message)
        {
            return InnerInvoke(message);
        }

        Task InnerInvoke(TransportMessage message, int currentIndex = 0)
        {
            if (currentIndex == executingSteps.Count)
            {
                return Task.CompletedTask;
            }

            IIncomingStep step = executingSteps[currentIndex];

            return step.Invoke(message, () => InnerInvoke(message, currentIndex + 1));
        }
    }
}