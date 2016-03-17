using AsyncDolls.Dequeuing;
using AsyncDolls.Pipeline.Incoming;
using AsyncDolls.Pipeline.Outgoing;

namespace AsyncDolls
{
    public class SendOnlyBus : Bus
    {
        public SendOnlyBus(SendOnlyConfiguration configuration, IOutgoingPipelineFactory outgoingPipelineFactory) : base(configuration, new NoOpDequeStrategy(), outgoingPipelineFactory, new EmptyIncomingPipelineFactory())
        {
        }
    }
}