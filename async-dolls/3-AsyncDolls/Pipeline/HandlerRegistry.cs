using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline
{
    public class HandlerRegistry : IHandlerRegistry
    {
        public virtual IReadOnlyCollection<object> GetHandlers(Type messageType)
        {
            return this.ConsumeAll();
        }

        public virtual Task InvokeHandle(object handler, object message, IBusForHandler bus)
        {
            dynamic h = handler;
            dynamic m = message;
            return h.Handle(m, bus);
        }
    }
}