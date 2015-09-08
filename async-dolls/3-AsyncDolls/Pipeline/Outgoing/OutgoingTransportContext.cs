namespace AsyncDolls.Pipeline.Outgoing
{
    public class OutgoingTransportContext : Context
    {
        const string OutgoingTransportMessageKey = "OutgoingTransportMessage";
        const string IncomingTransportMessageKey = "IncomingTransportMessage";

        public OutgoingTransportContext(LogicalMessage message, TransportMessage outgoingTransportMessage, DeliveryOptions options, EndpointConfiguration.ReadOnly configuration, TransportMessage incomingTransportMessage = null)
            : base(configuration)
        {
            Set(message);
            Set(OutgoingTransportMessageKey, outgoingTransportMessage);
            Set(IncomingTransportMessageKey, incomingTransportMessage);
            Set(options);
        }

        public LogicalMessage LogicalMessage
        {
            get { return Get<LogicalMessage>(); }
        }

        public TransportMessage OutgoingTransportMessage
        {
            get { return Get<TransportMessage>(OutgoingTransportMessageKey); }
        }

        public TransportMessage IncomingTransportMessage
        {
            get { return Get<TransportMessage>(IncomingTransportMessageKey); }
        }

        public DeliveryOptions Options
        {
            get { return Get<DeliveryOptions>(); }
        }
    }
}