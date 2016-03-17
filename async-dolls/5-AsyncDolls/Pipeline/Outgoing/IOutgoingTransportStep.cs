using System;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    using Properties;

    public interface IOutgoingTransportStep
    {
        Task Invoke([NotNull] OutgoingTransportContext context, [NotNull] Func<Task> next);
    }
}