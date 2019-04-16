using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions;
using Lykke.Logs;
using Lykke.Numerics;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class TransactionTests
    {
        [Test]
        public async Task CanSaveAndRetrieve()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferCoinsTransactionExecutedEvent();

            var btype = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(btype, evt);

            var retrieved = await repo.GetTransferCoinsTransactionAsync(btype, evt.TransactionId);

            Assert.AreEqual(evt.BlockId, retrieved.BlockId);
            Assert.AreEqual(evt.TransactionId, retrieved.TransactionId);
            Assert.AreEqual(evt.Fees.ToJson(), retrieved.Fees.ToJson());
            Assert.AreEqual(evt.ReceivedCoins.ToJson(), retrieved.ReceivedCoins.ToJson());
            Assert.AreEqual(evt.SpentCoins.ToJson(), evt.SpentCoins.ToJson());
        }

        [Test]
        public async Task CanUpdate()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferCoinsTransactionExecutedEvent();
            var evt2 = BuildRandomTransferCoinsTransactionExecutedEvent(evt.TransactionId);

            var btype = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(btype, evt);
            await repo.AddIfNotExistsAsync(btype, evt2);

            var retrieved = await repo.GetTransferCoinsTransactionAsync(btype, evt.TransactionId);

            Assert.AreNotEqual(evt.BlockId, retrieved.BlockId);
            Assert.AreNotEqual(evt.Fees.ToJson(), retrieved.Fees.ToJson());
            Assert.AreNotEqual(evt.ReceivedCoins.ToJson(), retrieved.ReceivedCoins.ToJson());
            Assert.AreNotEqual(evt.SpentCoins.ToJson(), retrieved.SpentCoins.ToJson());

            Assert.AreEqual(evt2.BlockId, retrieved.BlockId);
            Assert.AreEqual(evt2.TransactionId, retrieved.TransactionId);
            Assert.AreEqual(evt2.Fees.ToJson(), retrieved.Fees.ToJson());
            Assert.AreEqual(evt2.ReceivedCoins.ToJson(), retrieved.ReceivedCoins.ToJson());
            Assert.AreEqual(evt2.SpentCoins.ToJson(), retrieved.SpentCoins.ToJson());
        }


        [Test]
        public async Task CanDelete()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferCoinsTransactionExecutedEvent();

            var btype = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(btype, evt);

            var retrieved = await repo.GetTransferCoinsTransactionOrDefaultAsync(btype, evt.TransactionId);
            
            Assert.IsNotNull(retrieved);

            await repo.TryRemoveAllOfBlockAsync(btype, evt.BlockId);

            var retrieved2 = await repo.GetTransferCoinsTransactionOrDefaultAsync(btype, evt.TransactionId);
            Assert.IsNull(retrieved2);
        }

        [Test]
        public async Task CanCalcCount()
        {
            var repo = BuildRepo();

            var btype = Guid.NewGuid().ToString();
            var blockId = Guid.NewGuid().ToString();

            var insertedCount = 9;
            var ctr = 0;

            do
            {
                var evt = BuildRandomTransferCoinsTransactionExecutedEvent(blockId: blockId);

                await repo.AddIfNotExistsAsync(btype, evt);

                ctr++;
            } while (ctr<insertedCount);


            var counted = await repo.CountInBlockAsync(btype, blockId);

            Assert.AreEqual(counted, insertedCount);
        }


        private TransferCoinsTransactionExecutedEvent BuildRandomTransferCoinsTransactionExecutedEvent(string transactionId = null, string blockId = null)
        {
            var rnd = new Random();

            return new TransferCoinsTransactionExecutedEvent(
                blockId ??  Guid.NewGuid().ToString(),
                rnd.Next(), 
                transactionId ?? Guid.NewGuid().ToString(),
                new List<ReceivedCoin>
                {
                    BuildRandmonReceivedCoin(),
                    BuildRandmonReceivedCoin(),
                    BuildRandmonReceivedCoin()

                }, new List<CoinId>()
                {
                    BuildRandmomSpentCoin(),
                    BuildRandmomSpentCoin(),
                    BuildRandmomSpentCoin(),
                    BuildRandmomSpentCoin(),
                    BuildRandmomSpentCoin()
                }, new Fee[]
                {
                    BuildRandmomFee(),
                    BuildRandmomFee()
                });
        }

        private ReceivedCoin BuildRandmonReceivedCoin()
        {
            var rnd = new Random();
            return new ReceivedCoin(rnd.Next(), 
                new Asset(new AssetId(Guid.NewGuid().ToString())),
                new UMoney(new BigInteger(rnd.Next()), 12));
        }

        private CoinId BuildRandmomSpentCoin()
        {
            var rnd = new Random();

            return new CoinId(Guid.NewGuid().ToString(), rnd.Next());
        }

        private Fee BuildRandmomFee()
        {
            var rnd = new Random();
            
            return new Fee(new Asset(new AssetId(Guid.NewGuid().ToString())),
                new UMoney(new BigInteger(rnd.Next()), 123));
        }

        private TransactionsRepository BuildRepo()
        {
            return new TransactionsRepository(ContextFactory.GetPosgresTestsConnString(), EmptyLogFactory.Instance);
        }
    }
}
