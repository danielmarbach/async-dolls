namespace AsyncDolls.Testing
{
    using System;
    using System.Threading.Tasks;
    using AsyncDolls;
    using AsyncDolls.Pipeline.Outgoing;

    public class EnrichTransportMessageWithDestinationAddress : IOutgoingTransportStep
    {
        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (sendOptions != null)
            {
                context.OutgoingTransportMessage.Headers[AcceptanceTestHeaders.Destination] = sendOptions.Queue.ToString();
            }

            var publishOptions = context.Options as PublishOptions;
            if (publishOptions != null)
            {
                context.OutgoingTransportMessage.Headers[AcceptanceTestHeaders.Destination] = publishOptions.Topic.ToString();
            }

            await next();
        }
    }
}