using System;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    using Properties;

    public interface IIncomingLogicalStep
    {
        Task Invoke([NotNull] IncomingLogicalContext context, [NotNull] IBusForHandler bus, [NotNull] Func<Task> next);
    }
}