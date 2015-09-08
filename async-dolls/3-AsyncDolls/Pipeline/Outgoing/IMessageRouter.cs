using System;
using System.Collections.Generic;

namespace AsyncDolls.Pipeline.Outgoing
{
    public interface IMessageRouter
    {
        IReadOnlyCollection<Address> GetDestinationFor(Type messageType);
    }
}