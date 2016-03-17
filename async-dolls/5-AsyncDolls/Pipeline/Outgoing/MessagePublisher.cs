using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public class MessagePublisher : IPublishMessages
    {
        public Task PublishAsync(TransportMessage message, PublishOptions options)
        {
            return Task.CompletedTask;
        }

        public Task CloseAsync()
        {
            return Task.CompletedTask;
        }
    }
}