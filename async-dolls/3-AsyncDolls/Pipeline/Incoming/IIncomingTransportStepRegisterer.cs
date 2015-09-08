using System;

namespace AsyncDolls.Pipeline.Incoming
{
    public interface IIncomingTransportStepRegisterer
    {
        IIncomingTransportStepRegisterer Register(IIncomingTransportStep step);
        IIncomingTransportStepRegisterer Register(Func<IIncomingTransportStep> stepFactory);
    }
}