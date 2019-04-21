using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
using Lykke.Logs;
using Lykke.Numerics;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class TransactionTests
    {
        [Test]
        public async Task CanSaveAndRetrieveTransferCoins()
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


            Assert.AreEqual(evt.ToJson(), retrieved.ToJson());
        }

        [Test]
        public async Task CanSaveAndRetrieveTransferAmount()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferAmountEvent();

            var btype = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(btype, evt);

            var retrieved = await repo.GetTransferAmountTransactionAsync(btype, evt.TransactionId);

            Assert.AreEqual(evt.BlockId, retrieved.BlockId);
            Assert.AreEqual(evt.TransactionId, retrieved.TransactionId);
            Assert.AreEqual(evt.Fees.ToJson(), retrieved.Fees.ToJson());
            Assert.AreEqual(evt.BalanceChanges.ToJson(), retrieved.BalanceChanges.ToJson());

            Assert.AreEqual(evt.IsIrreversible, retrieved.IsIrreversible);


            Assert.AreEqual(evt.ToJson(), retrieved.ToJson());
        }

        [Test]
        public async Task CanSaveAndRetrieveFailedEvent()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferFailedEvent();

            var btype = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(btype, evt);

            var retrieved = await repo.GetFailedTransactionAsync(btype, evt.TransactionId);

            Assert.AreEqual(evt.ToJson(), retrieved.ToJson());
        }


        [Test]
        public async Task DoNotUpdates()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferCoinsTransactionExecutedEvent();
            var evt2 = BuildRandomTransferCoinsTransactionExecutedEvent(evt.TransactionId);

            var btype = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(btype, evt);
            await repo.AddIfNotExistsAsync(btype, evt2);

            var retrieved = await repo.GetTransferCoinsTransactionAsync(btype, evt.TransactionId);

            Assert.AreEqual(evt.BlockId, retrieved.BlockId);
            Assert.AreEqual(evt.Fees.ToJson(), retrieved.Fees.ToJson());
            Assert.AreEqual(evt.ReceivedCoins.ToJson(), retrieved.ReceivedCoins.ToJson());
            Assert.AreEqual(evt.SpentCoins.ToJson(), retrieved.SpentCoins.ToJson());
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

        public TransferAmountTransactionExecutedEvent BuildRandomTransferAmountEvent()
        {
            var rdm = new Random();
            return new TransferAmountTransactionExecutedEvent(new BlockId(Guid.NewGuid().ToString()),rdm.Next(), new TransactionId(Guid.NewGuid().ToString()),
                new BalanceChange[]{ BuuildRamdnomBalanceChange (),
                    BuuildRamdnomBalanceChange()},
                new []
                {
                    BuildRandmomFee(),
                    BuildRandmomFee(),
                    BuildRandmomFee()
                });
        }

        public TransactionFailedEvent BuildRandomTransferFailedEvent()
        {
            var rdm = new Random();
            return new TransactionFailedEvent(new BlockId(Guid.NewGuid().ToString()), rdm.Next(),
                new TransactionId(Guid.NewGuid().ToString()), TransactionBroadcastingError.NotEnoughBalance,
                Guid.NewGuid().ToString(),
                new Fee[]
                {
                    BuildRandmomFee()

                });
        }


        public BalanceChange BuuildRamdnomBalanceChange()
        {
            var rdm = new Random();
            return new BalanceChange(Guid.NewGuid().ToString(), new Asset(new AssetId(Guid.NewGuid().ToString())), new Money(rdm.Next(), 0), new Address(Guid.NewGuid().ToString()), new AddressTag(Guid.NewGuid().ToString()), AddressTagType.Number, rdm.Next());
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
