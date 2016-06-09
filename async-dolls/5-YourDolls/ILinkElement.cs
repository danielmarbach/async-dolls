using System;
using System.Threading.Tasks;

namespace AsyncDolls.YourDolls
{
    public interface ILinkElement
    {
        Task Invoke(TransportMessage transportMessage, Func<Task> next);
    }
}