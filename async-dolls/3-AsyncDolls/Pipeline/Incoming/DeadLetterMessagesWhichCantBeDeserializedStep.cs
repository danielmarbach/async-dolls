using System;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class DeadLetterMessagesWhichCantBeDeserializedStep : IIncomingTransportStep
    {
        private readonly IDeadLetterMessages deadLetter;

        public DeadLetterMessagesWhichCantBeDeserializedStep(IDeadLetterMessages deadLetter)
        {
            this.deadLetter = deadLetter;
        }

        public async Task Invoke(IncomingTransportContext context, IBusForHandler bus, Func<Task> next)
        {
            try
            {
                await next()
                    .ConfigureAwait(false);
            }
            catch (SerializationException exception)
            {
                var message = context.TransportMessage;

                message.SetFailureHeaders(exception, "Messages which can't be deserialized are deadlettered immediately");
                await deadLetter.DeadLetterAsync(message)
                    .ConfigureAwait(false);

                // Because we instructed the message to deadletter it is safe to rethrow. The broker will not redeliver.
                throw;
            }
        }

        static bool SerializationExceptionHasBeenCaught(ExceptionDispatchInfo serializationException)
        {
            return serializationException != null;
        }
    }
}