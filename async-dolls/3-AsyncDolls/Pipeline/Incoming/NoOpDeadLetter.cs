using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class NoOpDeadLetter : IDeadLetterMessages
    {
        public Task DeadLetterAsync(TransportMessage message)
        {
            return Task.CompletedTask;
        }
    }
}