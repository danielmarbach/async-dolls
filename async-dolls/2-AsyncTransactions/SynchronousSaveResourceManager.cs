using System;
using System.Threading.Tasks;
using System.Transactions;

namespace AsyncDolls
{
    internal class SynchronousSaveResourceManager : BaseResourceManager
    {
        private readonly Func<Task> operation;

        public SynchronousSaveResourceManager(Func<Task> operation)
        {
            this.operation = operation;
        }

        public override void Commit(Enlistment enlistment)
        {
            operation().Wait();

            enlistment.Done();
        }
    }
}