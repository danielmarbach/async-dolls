using System;
using System.Threading.Tasks;
using log4net;

namespace AsyncDolls.Dequeuing
{
    public class MessageReceiverReceiver : IReceiveMessages
    {
        static readonly ILog Logger = LogManager.GetLogger(typeof(MessageReceiverReceiver));

        public Task<AsyncClosable> StartAsync(
            EndpointConfiguration.ReadOnly configuration,
            Func<TransportMessage, Task> onMessage)
        {
            return Task.FromResult(new AsyncClosable(() => Task.CompletedTask));
        }
    }
}