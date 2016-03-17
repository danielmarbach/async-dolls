using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class DeserializeTransportMessageStep : IIncomingTransportStep
    {
        readonly LogicalMessageFactory factory;
        readonly IMessageSerializer serializer;

        public DeserializeTransportMessageStep(IMessageSerializer serializer)
        {
            factory = new LogicalMessageFactory();
            this.serializer = serializer;
        }

        public Task Invoke(IncomingTransportContext context, IBusForHandler bus, Func<Task> next)
        {
            var transportMessage = context.TransportMessage;

            try
            {
                context.Set(Extract(transportMessage));
            }
            catch (Exception exception)
            {
                throw new SerializationException(string.Format("An error occurred while attempting to extract logical messages from transport message {0}", transportMessage), exception);
            }

            return next();
        }

        LogicalMessage Extract(TransportMessage transportMessage)
        {
            Type messageType = Type.GetType(transportMessage.MessageType, true, true);

            object message = serializer.Deserialize(transportMessage.Body, messageType);

            return factory.Create(messageType, message, transportMessage.Headers);
        }
    }
}