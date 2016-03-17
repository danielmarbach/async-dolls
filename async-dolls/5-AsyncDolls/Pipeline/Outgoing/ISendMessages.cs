using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    public interface ISendMessages
    {
        Task SendAsync(TransportMessage message, SendOptions options);
    }
}