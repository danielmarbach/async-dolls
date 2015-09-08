using System;
using System.IO;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public class SerializeMessageStep : IOutgoingTransportStep
    {
        readonly IMessageSerializer serializer;

        public SerializeMessageStep(IMessageSerializer serializer)
        {
            this.serializer = serializer;
        }

        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            using (var ms = new MemoryStream())
            {
                serializer.Serialize(context.LogicalMessage.Instance, ms);

                context.OutgoingTransportMessage.ContentType = serializer.ContentType;
                context.OutgoingTransportMessage.MessageType = context.LogicalMessage.Instance.GetType().AssemblyQualifiedName;

                context.OutgoingTransportMessage.SetBody(ms);

                await next()
                    .ConfigureAwait(false);
            }
        }
    }
}