namespace AsyncDolls.AsyncStateWithDollsTyped
{
    public class IncomingLogicalContext : Context
    {
        public IncomingLogicalContext(LogicalMessage message, Context parent) : base(parent)
        {
            Set(message);
        }

        public LogicalMessage Message => Get<LogicalMessage>();
    }
}