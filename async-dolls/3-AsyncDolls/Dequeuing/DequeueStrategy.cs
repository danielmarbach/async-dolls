using System;
using System.Threading.Tasks;

namespace AsyncDolls.Dequeuing
{
    public class DequeueStrategy : IDequeueStrategy
    {
        readonly IReceiveMessages receiveMessages;
        EndpointConfiguration.ReadOnly configuration;
        Func<TransportMessage, Task> onMessageAsync;
        AsyncClosable receiver;

        public DequeueStrategy(IReceiveMessages receiveMessages)
        {
            this.receiveMessages = receiveMessages;
        }

        public async Task StartAsync(EndpointConfiguration.ReadOnly configuration, Func<TransportMessage, Task> onMessage)
        {
            this.configuration = configuration;
            onMessageAsync = onMessage;
            receiver = await receiveMessages.StartAsync(this.configuration, OnMessageAsync)
                .ConfigureAwait(false);
        }

        public Task StopAsync()
        {
            return receiver.CloseAsync();
        }

        async Task OnMessageAsync(TransportMessage message)
        {
            await onMessageAsync(message)
                .ConfigureAwait(false);
        }
    }
}