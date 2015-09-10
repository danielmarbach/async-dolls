using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class DeadLetterMessagesWhenRetryCountIsReachedStep : IIncomingLogicalStep
    {
        private readonly IDeadLetterMessages deadLetter;

        public DeadLetterMessagesWhenRetryCountIsReachedStep(IDeadLetterMessages deadLetter)
        {
            this.deadLetter = deadLetter;
        }

        public async Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
        {
            try
            {
                await next()
                    .ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                var message = context.TransportMessage;
                if (IsRetryCountReached(context))
                {
                    message.SetFailureHeaders(exception, "Max number of retries has been reached!");

                    // C# 6 can do this!
                    await deadLetter.DeadLetterAsync(message).ConfigureAwait(false);
                }
                else
                {
                    message.DeliveryCount++;
                    throw;
                }
            }
        }

        static bool IsRetryCountReached(IncomingLogicalContext context)
        {
            const int HardcodedMaxRetryOfServiceBusLibrary = 10;
            return context.TransportMessage.DeliveryCount > HardcodedMaxRetryOfServiceBusLibrary - 1;
        }
    }
}