namespace AsyncDolls.Pipeline.Incoming
{
    public class IncomingTransportContext : Context
    {
        public IncomingTransportContext(TransportMessage message, EndpointConfiguration.ReadOnly configuration)
            : base(configuration)
        {
            Set(message);
        }

        public TransportMessage TransportMessage
        {
            get { return Get<TransportMessage>(); }
        }
    }
}