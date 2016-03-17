using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline
{
    class AlwaysRejectMessageHandler : IHandleMessageAsync<object>
    {
        public Task Handle(object message, IBusForHandler bus)
        {
            IDictionary<string, string> headers = bus.Headers(message);

            var exceptionMessage = string.Format(CultureInfo.InvariantCulture, "The message with id {0} of type {1} has been rejected.", headers[HeaderKeys.MessageId], message.GetType());

            throw new InvalidOperationException(exceptionMessage);
        }
    }
}