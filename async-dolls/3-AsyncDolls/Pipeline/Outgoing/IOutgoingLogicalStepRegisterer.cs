using System;

namespace AsyncDolls.Pipeline.Outgoing
{
    public interface IOutgoingLogicalStepRegisterer
    {
        IOutgoingLogicalStepRegisterer Register(Func<IOutgoingLogicalStep> stepFactory);
        IOutgoingLogicalStepRegisterer Register(IOutgoingLogicalStep step);
    }
}