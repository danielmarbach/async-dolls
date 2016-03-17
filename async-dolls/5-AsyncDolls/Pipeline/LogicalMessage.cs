using System;
using System.Collections.Generic;

namespace AsyncDolls.Pipeline
{
    public class LogicalMessage
    {
        readonly IDictionary<string, string> headers;
        readonly object message;
        readonly Type messageType;

        public LogicalMessage(object message, IDictionary<string, string> headers)
            : this(message.GetType(), message, headers)
        {
        }

        public LogicalMessage(Type messageType, object message, IDictionary<string, string> headers)
        {
            this.messageType = messageType;
            this.message = message;
            this.headers = headers;
        }

        public Type MessageType
        {
            get { return messageType; }
        }

        public object Instance
        {
            get { return message; }
        }

        public IDictionary<string, string> Headers
        {
            get { return headers; }
        }
    }
}