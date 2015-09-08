using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    class EmptyIncomingPipelineFactory : IIncomingPipelineFactory
    {
        public Task WarmupAsync()
        {
            return Task.FromResult(0);
        }

        public IncomingPipeline Create()
        {
            return new IncomingPipeline();
        }

        public Task CooldownAsync()
        {
            return Task.FromResult(0);
        }
    }
}