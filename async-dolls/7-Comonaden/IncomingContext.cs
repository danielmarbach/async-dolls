namespace AsyncDolls.Comonaden
{
    public class IncomingContext : Context
    {
        public IncomingContext(TransportMessage message)
        {
            Set(message);
        }

        public TransportMessage Message => Get<TransportMessage>();
    }
}