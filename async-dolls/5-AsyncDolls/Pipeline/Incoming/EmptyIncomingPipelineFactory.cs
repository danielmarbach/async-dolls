using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    class EmptyIncomingPipelineFactory : IIncomingPipelineFactory
    {
        public Task WarmupAsync()
        {
            return Task.CompletedTask;
        }

        public IncomingPipeline Create()
        {
            return new IncomingPipeline();
        }

        public Task CooldownAsync()
        {
            return Task.CompletedTask;
        }
    }
}