using System.Collections.Generic;
using System.Runtime.ExceptionServices;

namespace AsyncDolls.AsyncDollsRecurseFree
{
    public class IncomingContext : Context
    {
        public IncomingContext(TransportMessage message)
        {
            Set(message);
            Exceptions = new Queue<ExceptionDispatchInfo>();
        }

        public TransportMessage Message => Get<TransportMessage>();

        public Queue<ExceptionDispatchInfo> Exceptions { get; }
    }
}