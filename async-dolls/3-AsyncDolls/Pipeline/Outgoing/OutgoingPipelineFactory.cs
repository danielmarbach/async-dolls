using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public class OutgoingPipelineFactory : IOutgoingPipelineFactory
    {
        readonly IMessageRouter router;
        MessagePublisher publisher;
        MessageSender sender;

        public OutgoingPipelineFactory(IMessageRouter router)
        {
            this.router = router;
        }

        public Task WarmupAsync()
        {
            sender = new MessageSender();
            publisher = new MessagePublisher();

            return Task.FromResult(0);
        }

        public OutgoingPipeline Create()
        {
            var pipeline = new OutgoingPipeline();

            pipeline.Logical
                .Register(new CreateTransportMessageStep());

            pipeline.Transport
                .Register(new SerializeMessageStep(new NewtonsoftJsonMessageSerializer()))
                .Register(new DetermineDestinationStep(router))
                .Register(new DispatchToTransportStep(sender, publisher));

            return pipeline;
        }

        public async Task CooldownAsync()
        {
            await sender.CloseAsync();
            await publisher.CloseAsync();
        }
    }
}