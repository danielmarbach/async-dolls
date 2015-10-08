using System;
using System.Collections.Generic;

namespace AsyncDollsSimple.Dequeuing
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
            var pipeline = new IncomingPipeline();

            foreach (var stepFactory in registeredStepFactories)
            {
                pipeline.Register(stepFactory());
            }

            return pipeline;
        }
    }
}