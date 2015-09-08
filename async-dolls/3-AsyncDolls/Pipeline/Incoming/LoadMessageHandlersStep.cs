using System;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class LoadMessageHandlersStep : IIncomingLogicalStep
    {
        readonly IHandlerRegistry registry;

        public LoadMessageHandlersStep(IHandlerRegistry registry)
        {
            this.registry = registry;
        }

        public async Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
        {
            var messageType = context.LogicalMessage.Instance.GetType();

            var handlers = registry.GetHandlers(messageType);

            foreach (var handler in handlers)
            {
                using (context.CreateSnapshot())
                {
                    var messageHandler = new MessageHandler
                    {
                        Instance = handler,
                        Invocation = (handlerInstance, message) => registry.InvokeHandle(handlerInstance, message, bus)
                    };

                    context.Handler = messageHandler;

                    await next()
                        .ConfigureAwait(false);

                    if (context.HandlerInvocationAbortPending)
                    {
                        break;
                    }
                }
            }
        }
    }
}