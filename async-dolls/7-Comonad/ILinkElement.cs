using System;
using System.Threading.Tasks;

namespace AsyncDolls.Comonaden
{
    public interface ILinkElement
    {
        Task<Continuation> Invoke(IncomingContext context);
    }
}