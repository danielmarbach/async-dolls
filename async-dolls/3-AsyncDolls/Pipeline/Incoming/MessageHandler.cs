using System;
using System.Threading.Tasks;

namespace AsyncDolls.Pipeline.Incoming
{
    public class MessageHandler
    {
        public object Instance { get; set; }
        public Func<object, object, Task> Invocation { get; set; }
    }
}