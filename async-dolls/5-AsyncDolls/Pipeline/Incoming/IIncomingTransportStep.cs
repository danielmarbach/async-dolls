using System;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    using Properties;

    public interface IIncomingTransportStep
    {
        Task Invoke([NotNull] IncomingTransportContext context, [NotNull] IBusForHandler bus, [NotNull] Func<Task> next);
    }
}