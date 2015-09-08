using System.Threading.Tasks;

namespace AsyncDolls
{
    using Properties;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public interface IHandleMessageAsync<in TMessage>
    {
        Task Handle([NotNull] TMessage message, [NotNull] IBusForHandler bus);
    }
}