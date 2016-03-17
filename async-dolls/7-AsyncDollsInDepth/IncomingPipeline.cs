using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsInDepth
{
    public class IncomingPipeline
    {
        readonly List<IIncomingStep> executingSteps;

        public IncomingPipeline(IEnumerable<IIncomingStep> steps)
        {
            executingSteps = new List<IIncomingStep>(steps);
        }

        public Task Invoke(IncomingContext context)
        {
            return InnerInvoke(context);
        }

        Task InnerInvoke(IncomingContext context, int currentIndex = 0)
        {
            if (currentIndex == executingSteps.Count)
            {
                return Task.CompletedTask;
            }

            IIncomingStep step = executingSteps[currentIndex];

            return step.Invoke(context, () => InnerInvoke(context, currentIndex + 1));
        }
    }
}