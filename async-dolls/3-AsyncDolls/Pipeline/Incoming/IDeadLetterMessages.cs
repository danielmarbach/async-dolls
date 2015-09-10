using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public interface IDeadLetterMessages
    {
        Task DeadLetterAsync(TransportMessage message);
    }
}