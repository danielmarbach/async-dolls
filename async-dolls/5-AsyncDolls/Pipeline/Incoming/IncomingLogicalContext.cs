namespace AsyncDolls.Pipeline.Incoming
{
    public class IncomingLogicalContext : Context
    {
        const string HandlerInvocationAbortPendingKey = "HandlerInvocationAbortPending";

        public IncomingLogicalContext(LogicalMessage logicalMessage, TransportMessage message, EndpointConfiguration.ReadOnly configuration)
            : base(configuration)
        {
            Set(logicalMessage);
            Set(message);
            Set<MessageHandler>(null, ShouldBeSnapshotted.Yes);
            Set(HandlerInvocationAbortPendingKey, false);
        }

        public LogicalMessage LogicalMessage
        {
            get { return Get<LogicalMessage>(); }
        }

        public TransportMessage TransportMessage
        {
            get { return Get<TransportMessage>(); }
        }

        public MessageHandler Handler
        {
            get { return Get<MessageHandler>(); }

            set { Set(value, ShouldBeSnapshotted.Yes); }
        }

        public bool HandlerInvocationAbortPending
        {
            get { return Get<bool>(HandlerInvocationAbortPendingKey); }
        }

        public void AbortHandlerInvocation()
        {
            Set(HandlerInvocationAbortPendingKey, true);
        }
    }
}