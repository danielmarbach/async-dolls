using System;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public class DispatchToTransportStep : IOutgoingTransportStep
    {
        readonly IPublishMessages publisher;
        readonly ISendMessages sender;

        public DispatchToTransportStep(ISendMessages sender, IPublishMessages publisher)
        {
            this.publisher = publisher;
            this.sender = sender;
        }

        public async Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (sendOptions != null)
            {
                await sender.SendAsync(context.OutgoingTransportMessage, sendOptions)
                    .ConfigureAwait(false);
            }

            var publishOptions = context.Options as PublishOptions;
            if (publishOptions != null)
            {
                await publisher.PublishAsync(context.OutgoingTransportMessage, publishOptions)
                    .ConfigureAwait(false);
            }

            await next()
                .ConfigureAwait(false);
        }
    }
}