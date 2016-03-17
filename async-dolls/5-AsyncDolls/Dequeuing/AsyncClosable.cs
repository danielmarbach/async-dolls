using System;
using System.Threading.Tasks;

namespace AsyncDolls.Dequeuing
{
    public class AsyncClosable
    {
        readonly Func<Task> closable;

        public AsyncClosable(Func<Task> closable)
        {
            this.closable = closable;
        }

        public Task CloseAsync()
        {
            return closable();
        }
    }
}