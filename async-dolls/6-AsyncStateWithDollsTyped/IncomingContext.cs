namespace AsyncDolls.AsyncStateWithDollsTyped
{
    public class IncomingPhysicalContext : Context
    {
        public IncomingPhysicalContext(TransportMessage message)
        {
            Set(message);
        }

        public TransportMessage Message => Get<TransportMessage>();
    }
}