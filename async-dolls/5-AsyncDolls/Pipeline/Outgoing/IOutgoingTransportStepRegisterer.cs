using System;

namespace AsyncDolls.Pipeline.Outgoing
{
    public interface IOutgoingTransportStepRegisterer
    {
        IOutgoingTransportStepRegisterer Register(Func<IOutgoingTransportStep> stepFactory);
        IOutgoingTransportStepRegisterer Register(IOutgoingTransportStep step);
    }
}