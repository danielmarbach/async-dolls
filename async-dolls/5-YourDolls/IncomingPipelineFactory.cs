using System;
using System.Collections.Generic;
using System.Linq;

namespace AsyncDolls.YourDolls
{
    public class IncomingPipelineFactory
    {
        private readonly Queue<Func<IIncomingStep>> registeredStepFactories = new Queue<Func<IIncomingStep>>();

        public IncomingPipelineFactory Register(Func<IIncomingStep> stepFactory)
        {
            registeredStepFactories.Enqueue(stepFactory);

            return this;
        }

        public IncomingPipeline Create()
        {
            var steps = registeredStepFactories.Select(stepFactory => stepFactory()).ToList();

            return new IncomingPipeline(steps);
        }
    }
}