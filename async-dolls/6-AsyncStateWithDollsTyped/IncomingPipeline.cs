using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncStateWithDollsTyped
{
    public class IncomingPipeline
    {
        readonly List<StepInstance> executingSteps;

        public IncomingPipeline(IEnumerable<IIncomingStep> steps)
        {
            executingSteps = steps.Select(s => new StepInstance(s)).ToList();
        }

        public Task Invoke(Context context)
        {
            return InnerInvoke(context);
        }

        Task InnerInvoke(Context context, int currentIndex = 0)
        {
            if (currentIndex == executingSteps.Count)
            {
                return Task.CompletedTask;
            }

            StepInstance step = executingSteps[currentIndex];

            return step.Invoke(context, ctx => InnerInvoke(ctx, currentIndex + 1));
        }
    }
}