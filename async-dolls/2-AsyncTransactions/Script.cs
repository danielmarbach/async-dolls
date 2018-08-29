using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Transactions;
using NUnit.Framework;

namespace AsyncDolls
{
    /// <summary>
    /// Contains a lot of white space. Optimized for Consolas 14 pt 
    /// and Full HD resolution
    /// </summary>
    [TestFixture]
    public class Script
    {
        [SetUp]
        public void SetUp()
        {
            Transaction.Current = null;

            foreach (string file in Directory.GetFiles(@".\", "*.received.txt"))
            {
                File.Delete(file);
            }
        }

        [Test]
        [TestCase("EntityFramework")]
        public void TheStart(string EntityFramework = null)
        {
            throw new ArgumentOutOfRangeException(nameof(EntityFramework), 
                "Sorry this presentation is not about Entity Framework");
        }







        [Test]
        public void TransactionScopeIntroOrRefresh()
        {
            using (var tx = new TransactionScope())
            {
                Assert.NotNull(Transaction.Current);

                SomeMethodInTheCallStack();

                tx.Complete();
            }

            Assert.Null(Transaction.Current);
        }

        private static void SomeMethodInTheCallStack()
        {
            Assert.NotNull(Transaction.Current);
        }



        [Test]
        public async Task TransactionScopeAsync()
        {
            Assert.Null(Transaction.Current);

            using (var tx = new TransactionScope())
            {
                Assert.NotNull(Transaction.Current);

                await SomeMethodInTheCallStackAsync()
                    .ConfigureAwait(false);

                tx.Complete();
            }

            Assert.Null(Transaction.Current);
        }

        [Test]
        public async Task TransactionScopeAsyncProper()
        {
            Assert.Null(Transaction.Current);

            using (var tx =
                new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                Assert.NotNull(Transaction.Current);

                await SomeMethodInTheCallStackAsync()
                    .ConfigureAwait(false);

                tx.Complete();
            }

            Assert.Null(Transaction.Current);
        }

        private static async Task SomeMethodInTheCallStackAsync()
        {
            await Task.Delay(500).ConfigureAwait(false);

            Assert.NotNull(Transaction.Current);
        }

        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [TestCase(DatabaseMode.Synchronous)]
        [TestCase(DatabaseMode.Dangerous)]
        [TestCase(DatabaseMode.AsyncBlocking)]
        public async Task StoreAsync(DatabaseMode mode)
        {
            var database = new Database("StoreAsync.received.txt", mode);
            StoringTenSwissGuysInTheDatabase(database);

            try
            {
                await database.SaveAsync().ConfigureAwait(false);
            }
            finally
            {
                database.Close();
            }
        }

        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [TestCase(DatabaseMode.Synchronous)]
        [TestCase(DatabaseMode.Dangerous)]
        [TestCase(DatabaseMode.AsyncBlocking)]
        public async Task StoreAsyncSupportsAmbientTransactionComplete(DatabaseMode mode)
        {
            var database = new Database("StoreAsyncSupportsAmbientTransactionComplete.received.txt", mode);
            StoringTenSwissGuysInTheDatabase(database);

            using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await database.SaveAsync().ConfigureAwait(false);

                tx.Complete();
            }

            database.Close();
        }

        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [TestCase(DatabaseMode.Synchronous)]
        [TestCase(DatabaseMode.Dangerous)]
        [TestCase(DatabaseMode.AsyncBlocking)]
        public async Task StoreAsyncSupportsAmbientTransactionRollback(DatabaseMode mode)
        {
            var database = new Database("StoreAsyncSupportsAmbientTransactionRollback.received.txt", mode);
            StoringTenSwissGuysInTheDatabase(database);

            using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await database.SaveAsync().ConfigureAwait(false);

                // Rollback
            }

            database.Close();
        }

        private static void StoringTenSwissGuysInTheDatabase(Database database)
        {
            for (int i = 0; i < 10; i++)
            {
                database.Store(new Customer {Name = "Daniel" + i});
            }
        }
    }
}