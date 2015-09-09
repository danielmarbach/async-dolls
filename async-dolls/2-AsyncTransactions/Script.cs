using System;
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
        }




        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        [TestCase("EntityFramework")]
        public void TheStart(string EntityFramework = null)
        {
            throw new ArgumentOutOfRangeException(nameof(EntityFramework), 
                "Sorry this presentation is not about Entity Framework");
        }







        [Test]
        public async Task TransactionScopeIntroOrRefresh()
        {
            var slide = new Slide(title: "Transaction Scope Intro");
            await slide
                .BulletPoint("System.Transactions.TransactionScope")
                .BulletPoint("Implicit programing model / Ambient Transactions")
                .BulletPoint("Only works with async/await in .NET 4.5.1")
                .BulletPoint("Limited usefulness in cloud scenarios")
                .BulletPoint("Upgrades a local transaction to a distributed transaction")
                .BulletPoint("Ease of coding - If you prefer implicit over explicit")












                .Sample(() =>
                {
                    Assert.Null(Transaction.Current);

                    using (var tx = new TransactionScope())
                    {
                        Assert.NotNull(Transaction.Current);

                        SomeMethodInTheCallStack();

                        tx.Complete();
                    }

                    Assert.Null(Transaction.Current);
                });
        }

        private static void SomeMethodInTheCallStack()
        {
            Assert.NotNull(Transaction.Current);
        }



        [Test]
        [ExpectedException(typeof(InvalidOperationException))] // Normally never use those
        public async Task TransactionScopeAsync()
        {
            var slide = new Slide(title: "Transaction Scope Async");
            await slide

                .Sample(async () =>
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
                });
        }

        [Test]
        public async Task TransactionScopeAsyncProper()
        {
            var slide = new Slide(title: "Transaction Scope Async");
            await slide
                .Sample(async () =>
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
                });
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
            var slide = new Slide(title: "Store Async");
            await slide
                .BulletPoint("OMG! You just rolled your own NoSQL database, right?")

                .Sample(async () =>
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
                });
        }

        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [TestCase(DatabaseMode.Synchronous)]
        [TestCase(DatabaseMode.Dangerous)]
        [TestCase(DatabaseMode.AsyncBlocking)]
        public async Task StoreAsyncSupportsAmbientTransactionComplete(DatabaseMode mode)
        {
            var slide = new Slide(title: "Store Async supports ambient transactions - complete");
            await slide
                .Sample(async () =>
                {
                    var database = new Database("StoreAsyncSupportsAmbientTransactionComplete.received.txt", mode);
                    StoringTenSwissGuysInTheDatabase(database);

                    using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        await database.SaveAsync().ConfigureAwait(false);

                        tx.Complete();
                    }

                    database.Close();
                });
        }

        [Test]
        [MethodImpl(MethodImplOptions.NoInlining)]
        [TestCase(DatabaseMode.Synchronous)]
        [TestCase(DatabaseMode.Dangerous)]
        [TestCase(DatabaseMode.AsyncBlocking)]
        public async Task StoreAsyncSupportsAmbientTransactionRollback(DatabaseMode mode)
        {
            var slide = new Slide(title: "Store Async supports ambient transactions - rollback");
            await slide
                .Sample(async () =>
                {
                    var database = new Database("StoreAsyncSupportsAmbientTransactionRollback.received.txt", mode);
                    StoringTenSwissGuysInTheDatabase(database);

                    using (var tx = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        await database.SaveAsync().ConfigureAwait(false);

                        // Rollback
                    }

                    database.Close();
                });
        }




        [Test]
        public async Task WhatDidWeJustLearn()
        {
            var slide = new Slide(title: "What did we just learn?");
            await slide
                .BulletPoint("Async void is evil! Use carefully.")
                .BulletPoint("Async all the way.")
                .BulletPoint("In a cloud first and async world try to avoid TransactionScope")
                .BulletPoint("Modern frameworks like EF6 (and higher) support own transactions")
                .BulletPoint("Use AsyncPump if you need TxScope and enlist your own stuff.")
                .BulletPoint("Write an async enabled library? ConfigureAwait(false) is your friend");
        }









        [Test]
        [Explicit]
        public async Task TheEnd()
        {
            var giveAway = new GiveAway();
            await giveAway.WorthThousandDollars();
        }







        [Test]
        public async Task Links()
        {
            var slide = new Slide(title: "Useful links");
            await slide
                .BulletPoint("Sample Code including Transcript of what I explained" +
                             "https://github.com/danielmarbach/AsyncTransactions")
                .BulletPoint("Six Essential Tips for Async - " +
                             "http://channel9.msdn.com/Series/Three-Essential-Tips-for-Async")
                .BulletPoint("Best Practices in Asynchronous Programming" +
                             "https://msdn.microsoft.com/en-us/magazine/jj991977.aspx")
                .BulletPoint("Participating in TransactionScopes and Async/Await" +
                             "http://www.planetgeek.ch/2014/12/07/participating-in-transactionscopes-and-asyncawait-introduction/")
                .BulletPoint("Working with Transactions (EF6 Onwards)" +
                             "https://msdn.microsoft.com/en-us/data/dn456843.aspx")
                .BulletPoint("Enlisting Resources as Participants in a Transaction" +
                             "https://msdn.microsoft.com/en-us/library/ms172153.aspx");
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