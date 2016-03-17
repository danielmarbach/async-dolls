using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public interface IIncomingPipelineFactory
    {
        Task WarmupAsync();
        IncomingPipeline Create();
        Task CooldownAsync();
    }
}