using System;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class RetryMessagesStep : IIncomingLogicalStep
    {
        public Task Invoke(IncomingLogicalContext context, IBusForHandler bus, Func<Task> next)
        {
            var delay = 100;
            return InvokeWithDelay(context, next, delay);
        }

        private static async Task InvokeWithDelay(IncomingLogicalContext context, Func<Task> next, int delay)
        {
            try
            {
                await next().ConfigureAwait(false);
            }
            catch (Exception)
            {
                delay += 100;
                await Task.Delay(delay);
                await InvokeWithDelay(context, next, delay);
            }
        }
    }
}