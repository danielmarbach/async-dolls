using System;
using System.Threading.Tasks;

namespace AsyncDolls.Comonaden
{
    public interface IIncomingStep
    {
        Task<Continuation> Invoke(IncomingContext context);
    }
}