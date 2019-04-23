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

            var blockchainType = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(blockchainType, evt);

            var envelope = await repo.GetAsync(blockchainType, evt.TransactionId);

            Assert.IsTrue(envelope.IsTransferCoins);

            var transaction = envelope.AsTransferCoins();

            Assert.AreEqual(evt.BlockId, transaction.BlockId);
            Assert.AreEqual(evt.TransactionId, transaction.TransactionId);
            Assert.AreEqual(evt.Fees.ToJson(), transaction.Fees.ToJson());
            Assert.AreEqual(evt.ReceivedCoins.ToJson(), transaction.ReceivedCoins.ToJson());
            Assert.AreEqual(evt.SpentCoins.ToJson(), transaction.SpentCoins.ToJson());
            
            Assert.AreEqual(evt.ToJson(), transaction.ToJson());
        }

        [Test]
        public async Task CanSaveAndRetrieveTransferAmount()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferAmountEvent();

            var blockchainType = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(blockchainType, evt);

            var envelope = await repo.GetAsync(blockchainType, evt.TransactionId);

            Assert.IsTrue(envelope.IsTransferAmount);

            var transaction = envelope.AsTransferAmount();

            Assert.AreEqual(evt.BlockId, transaction.BlockId);
            Assert.AreEqual(evt.TransactionId, transaction.TransactionId);
            Assert.AreEqual(evt.Fees.ToJson(), transaction.Fees.ToJson());
            Assert.AreEqual(evt.BalanceChanges.ToJson(), transaction.BalanceChanges.ToJson());

            Assert.AreEqual(evt.IsIrreversible, transaction.IsIrreversible);
            
            Assert.AreEqual(evt.ToJson(), transaction.ToJson());
        }

        [Test]
        public async Task CanSaveAndRetrieveFailedEvent()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferFailedEvent();

            var blockchainType = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(blockchainType, evt);

            var envelope = await repo.GetAsync(blockchainType, evt.TransactionId);

            Assert.IsTrue(envelope.IsFailed);

            var transaction = envelope.AsFailed();

            Assert.AreEqual(evt.ToJson(), transaction.ToJson());
        }
        
        [Test]
        public async Task DoNotUpdates()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferCoinsTransactionExecutedEvent();
            var evt2 = BuildRandomTransferCoinsTransactionExecutedEvent(evt.TransactionId);

            var blockchainType = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(blockchainType, evt);
            await repo.AddIfNotExistsAsync(blockchainType, evt2);

            var envelope = await repo.GetAsync(blockchainType, evt.TransactionId);

            Assert.IsTrue(envelope.IsTransferCoins);

            var transaction = envelope.AsTransferCoins();

            Assert.AreEqual(evt.BlockId, transaction.BlockId);
            Assert.AreEqual(evt.Fees.ToJson(), transaction.Fees.ToJson());
            Assert.AreEqual(evt.ReceivedCoins.ToJson(), transaction.ReceivedCoins.ToJson());
            Assert.AreEqual(evt.SpentCoins.ToJson(), transaction.SpentCoins.ToJson());
        }


        [Test]
        public async Task CanDelete()
        {
            var repo = BuildRepo();

            var evt = BuildRandomTransferCoinsTransactionExecutedEvent();

            var blockchainType = Guid.NewGuid().ToString();
            await repo.AddIfNotExistsAsync(blockchainType, evt);

            var envelope = await repo.GetOrDefaultAsync(blockchainType, evt.TransactionId);
            
            Assert.IsNotNull(envelope);
            Assert.IsTrue(envelope.IsTransferCoins);
            Assert.IsNotNull(envelope.AsTransferCoins());

            await repo.TryRemoveAllOfBlockAsync(blockchainType, evt.BlockId);

            var envelope2 = await repo.GetOrDefaultAsync(blockchainType, evt.TransactionId);
            
            Assert.IsNull(envelope2);
        }

        [Test]
        public async Task CanCalcCount()
        {
            var repo = BuildRepo();

            var blockchainType = Guid.NewGuid().ToString();
            var blockId = Guid.NewGuid().ToString();

            var insertedCount = 9;
            var ctr = 0;

            do
            {
                var evt = BuildRandomTransferCoinsTransactionExecutedEvent(blockId: blockId);

                await repo.AddIfNotExistsAsync(blockchainType, evt);

                ctr++;
            } while (ctr<insertedCount);


            var counted = await repo.CountInBlockAsync(blockchainType, blockId);

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
                    BuildRandomReceivedCoin(),
                    BuildRandomReceivedCoin(),
                    BuildRandomReceivedCoin()

                }, new List<CoinId>
                {
                    BuildRandomSpentCoin(),
                    BuildRandomSpentCoin(),
                    BuildRandomSpentCoin(),
                    BuildRandomSpentCoin(),
                    BuildRandomSpentCoin()
                }, new[]
                {
                    BuildRandomFee(),
                    BuildRandomFee()
                });
        }

        private TransferAmountTransactionExecutedEvent BuildRandomTransferAmountEvent()
        {
            var rdm = new Random();
            return new TransferAmountTransactionExecutedEvent(new BlockId(Guid.NewGuid().ToString()),rdm.Next(), new TransactionId(Guid.NewGuid().ToString()),
                new[]{ BuildRandomBalanceChange (),
                    BuildRandomBalanceChange()},
                new []
                {
                    BuildRandomFee(),
                    BuildRandomFee(),
                    BuildRandomFee()
                });
        }

        private static TransactionFailedEvent BuildRandomTransferFailedEvent()
        {
            var rdm = new Random();
            return new TransactionFailedEvent(new BlockId(Guid.NewGuid().ToString()), rdm.Next(),
                new TransactionId(Guid.NewGuid().ToString()), TransactionBroadcastingError.NotEnoughBalance,
                Guid.NewGuid().ToString(),
                new[]
                {
                    BuildRandomFee()

                });
        }


        private static BalanceChange BuildRandomBalanceChange()
        {
            var rdm = new Random();
            return new BalanceChange(Guid.NewGuid().ToString(), new Asset(new AssetId(Guid.NewGuid().ToString())), new Money(rdm.Next(), 0), new Address(Guid.NewGuid().ToString()), new AddressTag(Guid.NewGuid().ToString()), AddressTagType.Number, rdm.Next());
        }

        private static ReceivedCoin BuildRandomReceivedCoin()
        {
            var rnd = new Random();
            return new ReceivedCoin(rnd.Next(), 
                new Asset(new AssetId(Guid.NewGuid().ToString())),
                new UMoney(new BigInteger(rnd.Next()), 12));
        }

        private static CoinId BuildRandomSpentCoin()
        {
            var rnd = new Random();

            return new CoinId(Guid.NewGuid().ToString(), rnd.Next());
        }

        private static Fee BuildRandomFee()
        {
            var rnd = new Random();
            
            return new Fee(new Asset(new AssetId(Guid.NewGuid().ToString())),
                new UMoney(new BigInteger(rnd.Next()), 123));
        }

        private static TransactionsRepository BuildRepo()
        {
            return new TransactionsRepository(ContextFactory.GetPosgresTestsConnString());
        }
    }
}
