using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public interface IPublishMessages
    {
        Task PublishAsync(TransportMessage message, PublishOptions options);
    }
}