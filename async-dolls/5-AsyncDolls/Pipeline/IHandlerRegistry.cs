using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline
{
    public interface IHandlerRegistry
    {
        IReadOnlyCollection<object> GetHandlers(Type messageType);
        Task InvokeHandle(object handler, object message, IBusForHandler bus);
    }
}