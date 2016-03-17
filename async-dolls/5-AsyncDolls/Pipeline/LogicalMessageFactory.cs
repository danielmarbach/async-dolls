using System;
using System.Collections.Generic;

namespace AsyncDolls.Pipeline
{
    class LogicalMessageFactory
    {
        public LogicalMessage Create(object message, IDictionary<string, string> headers)
        {
            return Create(message.GetType(), message, headers);
        }

        public LogicalMessage Create(Type messageType, object message, IDictionary<string, string> headers)
        {
            if (message == null)
            {
                throw new ArgumentNullException("message");
            }

            if (messageType == null)
            {
                throw new ArgumentNullException("messageType");
            }

            if (headers == null)
            {
                throw new ArgumentNullException("headers");
            }

            return new LogicalMessage(messageType, message, headers);
        }
    }
}