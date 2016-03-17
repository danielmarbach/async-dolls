using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public interface IOutgoingPipelineFactory
    {
        Task WarmupAsync();
        OutgoingPipeline Create();
        Task CooldownAsync();
    }
}