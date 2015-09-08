using System.Threading.Tasks;

namespace AsyncDolls
{
    public interface IBus
    {
        /// <summary>
        ///     Sends the message back to the current bus.
        /// </summary>
        /// <param name="message">The message to send.</param>
        Task SendLocal(object message);

        Task Send(object message, SendOptions options = null);
        Task Publish(object message, PublishOptions options = null);
    }
}