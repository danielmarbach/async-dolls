using System;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class InvokeHandlerStep : IIncomingLogicalStep
    {
        public async Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
        {
            var messageHandler = context.Handler;

            await messageHandler.Invocation(messageHandler.Instance, context.LogicalMessage.Instance)
                .ConfigureAwait(false);

            await next()
                .ConfigureAwait(false);
        }
    }
}