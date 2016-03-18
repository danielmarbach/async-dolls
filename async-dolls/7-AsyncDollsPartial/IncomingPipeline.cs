using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncDolls.AsyncDollsPartial
{
    public class IncomingPipeline
    {
        readonly List<StepInstance> executingSteps;
        private Stack<Tuple<Context, StepInstance>> afterSteps;

        public IncomingPipeline(IEnumerable<IIncomingStep> steps)
        {
            executingSteps = steps.Select(s => new StepInstance(s)).ToList();
            afterSteps = new Stack<Tuple<Context, StepInstance>>();
        }

        public Task Invoke(Context context)
        {
            return InnerInvoke(context, new Index());
        }

        async Task InnerInvoke(Context context, Index index)
        {
            StepInstance step;
            for (int i = index.Value; i < executingSteps.Count; i++)
            {
                index.Value = i;
                step = executingSteps[index.Value];
                if (step.IsBefore)
                {
                    await step.Invoke(context, ctx => Task.CompletedTask).ConfigureAwait(false);
                    continue;
                }

                if (step.IsSurround)
                {
                    index.Value += 1;
                    await step.Invoke(context, ctx => InnerInvoke(ctx, index)).ConfigureAwait(false);
                    i = index.Value++;
                    continue;
                }

                if (step.IsAfter)
                {
                    afterSteps.Push(Tuple.Create(context, step));
                }
            }

            if (index.Value == executingSteps.Count)
            {
                foreach (var contextAndStep in afterSteps)
                {
                    await contextAndStep.Item2.Invoke(contextAndStep.Item1, ctx => Task.CompletedTask).ConfigureAwait(false);
                }
            }
        }

        class Index
        {
            public int Value;
        }
    }
}