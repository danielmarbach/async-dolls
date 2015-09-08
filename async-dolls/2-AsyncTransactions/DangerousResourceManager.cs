using System;
using System.Threading.Tasks;
using System.Transactions;

namespace AsyncDolls
{
    internal class DangerousResourceManager : BaseResourceManager
    {
        private readonly Func<Task> operation;

        public DangerousResourceManager(Func<Task> operation)
        {
            this.operation = operation;
        }

        public override async void Commit(Enlistment enlistment)
        {
            await operation();

            enlistment.Done();
        }
    }
}