using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public class DetermineDestinationStep : IOutgoingTransportStep
    {
        readonly IMessageRouter router;

        public DetermineDestinationStep(IMessageRouter router)
        {
            this.router = router;
        }

        public Task Invoke(OutgoingTransportContext context, Func<Task> next)
        {
            var sendOptions = context.Options as SendOptions;
            if (ShouldDetermineSendDestination(sendOptions))
            {
// ReSharper disable PossibleNullReferenceException
                sendOptions.Queue = GetDestinationForSend(context.LogicalMessage.Instance);
// ReSharper restore PossibleNullReferenceException
            }

            var publishOptions = context.Options as PublishOptions;
            if (ShouldDeterminePublishDestination(publishOptions))
            {
// ReSharper disable PossibleNullReferenceException
                publishOptions.Topic = GetDestinationForPublish(context.LogicalMessage.Instance);
// ReSharper restore PossibleNullReferenceException
            }

            return next();
        }

        static bool ShouldDeterminePublishDestination(PublishOptions publishOptions)
        {
            return publishOptions != null && publishOptions.Topic == null;
        }

        static bool ShouldDetermineSendDestination(SendOptions sendOptions)
        {
            return sendOptions != null && sendOptions.Queue == null;
        }

        Queue GetDestinationForSend(object message)
        {
            IReadOnlyCollection<Address> destinations = router.GetDestinationFor(message.GetType());

            if (destinations.Count > 1)
            {
                throw new InvalidOperationException("Sends can only have one target address.");
            }

            return destinations.OfType<Queue>().Single();
        }

        Topic GetDestinationForPublish(object message)
        {
            IReadOnlyCollection<Address> destinations = router.GetDestinationFor(message.GetType());

            if (destinations.Count > 1)
            {
                throw new InvalidOperationException("Publish can only have one target address.");
            }

            return destinations.OfType<Topic>().Single();
        }
    }
}