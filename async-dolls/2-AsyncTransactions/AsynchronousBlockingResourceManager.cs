using System;
using System.Threading.Tasks;
using System.Transactions;

namespace AsyncDolls
{
    internal class AsynchronousBlockingResourceManager : BaseResourceManager
    {
        private readonly Func<Task> operation;

        public AsynchronousBlockingResourceManager(Func<Task> operation)
        {
            this.operation = operation;
        }

        public override void Commit(Enlistment enlistment)
        {
            AsyncPump.Run(operation);

            enlistment.Done();
        }
    }
}