using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDollsSimple.Dequeuing
{
    public class IncomingPipeline
    {
        readonly Queue<IIncomingStep> registeredSteps;
        Queue<IIncomingStep> executingSteps;

        public IncomingPipeline()
        {
            registeredSteps = new Queue<IIncomingStep>();
        }

        public IncomingPipeline Register(IIncomingStep step)
        {
            registeredSteps.Enqueue(step);

            return this;
        }

        public Task Invoke(TransportMessage message)
        {
            executingSteps = new Queue<IIncomingStep>(registeredSteps);
            return InnerInvoke(message);
        }

        Task InnerInvoke(TransportMessage message)
        {
            if (executingSteps.Count == 0)
            {
                return Task.CompletedTask;
            }

            IIncomingStep step = executingSteps.Dequeue();

            return step.Invoke(message, () => InnerInvoke(message));
        }
    }
}