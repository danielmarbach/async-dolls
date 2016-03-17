using System;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Outgoing
{
    using Properties;

    public interface IOutgoingLogicalStep
    {
        Task Invoke([NotNull] OutgoingLogicalContext context, [NotNull] Func<Task> next);
    }
}