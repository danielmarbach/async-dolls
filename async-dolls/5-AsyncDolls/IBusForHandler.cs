using System.Collections.Generic;
using System.Threading.Tasks;

namespace AsyncDolls
{
    public interface IBusForHandler : IBus
    {
        Task Reply(object message, ReplyOptions options = null);
        IDictionary<string, string> Headers(object message);
        void DoNotContinueDispatchingCurrentMessageToHandlers();
    }
}