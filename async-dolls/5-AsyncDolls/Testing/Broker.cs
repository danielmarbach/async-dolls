namespace AsyncDolls.Testing
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AsyncDolls;

    public class Broker
    {
        readonly Dictionary<Address, IList<MessageUnit>> units;

        public Broker()
        {
            units = new Dictionary<Address, IList<MessageUnit>>();
        }

        public Broker Register(MessageUnit unit)
        {
            return Register(unit, unit.Endpoint);
        }

        public Broker Register(MessageUnit unit, Topic topic)
        {
            return Register(unit, (Address) topic);
        }

        public async Task StartAsync()
        {
            foreach (MessageUnit unit in units.SelectMany(x => x.Value))
            {
                await unit.StartAsync()
                    .ConfigureAwait(false);
            }
        }

        public async Task StopAsync()
        {
            foreach (MessageUnit unit in units.SelectMany(x => x.Value).Reverse())
            {
                await unit.StopAsync()
                    .ConfigureAwait(false);
            }
        }

        Broker Register(MessageUnit unit, Address address)
        {
            if (!units.ContainsKey(address))
            {
                units.Add(address, new List<MessageUnit>());

                unit.SetOutgoing(Outgoing);
                units[address].Add(unit);
            }

            return this;
        }

        async Task Outgoing(TransportMessage message)
        {
            var address = message.Headers[AcceptanceTestHeaders.Destination].Parse();

            IList<MessageUnit> destinations;
            if (!units.TryGetValue(address, out destinations))
            {
                destinations = new MessageUnit[]
                {
                };
            }

            foreach (MessageUnit unit in destinations)
            {
                await unit.HandOver(message)
                    .ConfigureAwait(false);
            }
        }
    }
}