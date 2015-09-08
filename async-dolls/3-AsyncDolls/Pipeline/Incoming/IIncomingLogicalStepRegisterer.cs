using System;

namespace AsyncDolls.Pipeline.Incoming
{
    public interface IIncomingLogicalStepRegisterer
    {
        IIncomingLogicalStepRegisterer Register(IIncomingLogicalStep step);
        IIncomingLogicalStepRegisterer Register(Func<IIncomingLogicalStep> stepFactory);
    }
}