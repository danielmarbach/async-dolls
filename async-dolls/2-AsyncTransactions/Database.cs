using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Transactions;
using Newtonsoft.Json;

namespace AsyncDolls
{
    public class Database
    {
        readonly List<object> stored = new List<object>();
        private readonly DatabaseStore store;
        private readonly Func<Transaction, Task> saveUnderTxAsync;
        private readonly DatabaseMode mode;

        public Database(string storePath, DatabaseMode mode = DatabaseMode.Synchronous)
        {
            store = new DatabaseStore(storePath);
            this.mode = mode;

            switch (mode)
            {
                case DatabaseMode.Synchronous:
                    saveUnderTxAsync = tx => 
                    Task.FromResult(tx.EnlistVolatile(new SynchronousSaveResourceManager(SaveInternalAsync), EnlistmentOptions.None));
                    break;
                case DatabaseMode.Dangerous:
                    saveUnderTxAsync = tx => 
                    Task.FromResult(tx.EnlistVolatile(new DangerousResourceManager(SaveInternalAsync), EnlistmentOptions.None));
                    break;
                case DatabaseMode.AsyncBlocking:
                    saveUnderTxAsync = tx => 
                    Task.FromResult(tx.EnlistVolatile(new AsynchronousBlockingResourceManager(SaveInternalAsync), EnlistmentOptions.None));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Store(object entity)
        {
            stored.Add(entity);            
        }

        public Task SaveAsync()
        {
            return SaveAsync(Transaction.Current);
        }

        public async Task SaveAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                await SaveInternalAsync();
                return;
            }

            await saveUnderTxAsync(transaction);
        }

        private async Task SaveInternalAsync()
        {
            foreach (var o in stored)
            {
                if(mode == DatabaseMode.Dangerous)
                    throw new DirectoryNotFoundException();

                using(var stream = new MemoryStream())
                using (var writer = new JsonTextWriter(new StreamWriter(stream)))
                {
                    var serializer = JsonSerializer.Create();
                    serializer.Serialize(writer, o);
                    writer.Flush();
                    stream.Position = 0;

                    await store.AppendAsync(stream);

                    writer.Close();
                }
            }
            stored.Clear();
        }

        public void Close()
        {
            store.Close();
        }
    }
}
