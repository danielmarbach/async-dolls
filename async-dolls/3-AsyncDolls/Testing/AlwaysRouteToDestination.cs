namespace AsyncDolls.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AsyncDolls;
    using AsyncDolls.Pipeline.Outgoing;

    public class AlwaysRouteToDestination : IMessageRouter
    {
        readonly Address destination;

        public AlwaysRouteToDestination(Address destination)
        {
            this.destination = destination;
        }

        public IReadOnlyCollection<Address> GetDestinationFor(Type messageType)
        {
            var addresses = new List<Address>
            {
                destination
            };
            return new ReadOnlyCollection<Address>(addresses);
        }
    }
}