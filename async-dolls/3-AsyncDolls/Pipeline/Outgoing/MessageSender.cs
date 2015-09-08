using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public class MessageSender : ISendMessages
    {
        public Task SendAsync(TransportMessage message, SendOptions options)
        {
            return Task.FromResult(0);
        }

        public Task CloseAsync()
        {
            return Task.FromResult(0);
        }
    }
}