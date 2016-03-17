using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class IncomingPipelineFactory : IIncomingPipelineFactory
    {
        readonly IHandlerRegistry registry;

        public IncomingPipelineFactory(IHandlerRegistry registry)
        {
            this.registry = registry;
        }

        public Task WarmupAsync()
        {
            return Task.CompletedTask;
        }

        public IncomingPipeline Create()
        {
            var pipeline = new IncomingPipeline();

            pipeline.Transport
                .Register(new DeadLetterMessagesWhichCantBeDeserializedStep(new NoOpDeadLetter()))
                .Register(new DeserializeTransportMessageStep(new NewtonsoftJsonMessageSerializer()));

            pipeline.Logical
                .Register(new DeadLetterMessagesWhenRetryCountIsReachedStep(new NoOpDeadLetter()))
                .Register(new LoadMessageHandlersStep(registry))
                .Register(new InvokeHandlerStep());

            return pipeline;
        }

        public Task CooldownAsync()
        {
            return Task.CompletedTask;
        }
    }
}