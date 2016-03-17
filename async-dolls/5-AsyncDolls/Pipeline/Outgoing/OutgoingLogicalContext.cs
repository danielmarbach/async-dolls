namespace AsyncDolls.Pipeline.Outgoing
{
    public class OutgoingLogicalContext : Context
    {
        public OutgoingLogicalContext(LogicalMessage message, DeliveryOptions options, EndpointConfiguration.ReadOnly configuration)
            : base(configuration)
        {
            Set(message);
            Set(options);
        }

        public LogicalMessage LogicalMessage
        {
            get { return Get<LogicalMessage>(); }
        }

        public DeliveryOptions Options
        {
            get { return Get<DeliveryOptions>(); }
        }
    }
}