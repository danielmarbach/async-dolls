using System;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace AsyncDolls.Comonaden
{
    public class Continuation
    {
        public Func<Task> After { get; set; } = () => Task.CompletedTask;
        public Func<Task> Finally { get; set; } = () => Task.CompletedTask;

        public Func<ExceptionDispatchInfo, Task<ExceptionDispatchInfo>> Catch { get; set; }

        public static Continuation Empty = new Continuation();

        public static Task<Continuation> Completed = Empty;

        public static implicit operator Task<Continuation>(Continuation continuation)
        {
            return Task.FromResult(continuation);
        }
    }
}