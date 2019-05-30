using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Common;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
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

            var transaction = BuildRandomTransferCoinsExecutedTransaction();
            var transferCoinsTransaction = transaction.AsTransferCoins();

            await repo.AddIfNotExistsAsync(new[] {transaction});

            var readTransaction = await repo.GetAsync(transaction.BlockchainType, transferCoinsTransaction.TransactionId);

            Assert.IsTrue(readTransaction.IsTransferCoins);

            var readTransferCoinsTransaction = readTransaction.AsTransferCoins();

            Assert.AreEqual(transaction.BlockId, readTransaction.BlockId);
            Assert.AreEqual(transferCoinsTransaction.TransactionId, readTransferCoinsTransaction.TransactionId);
            CollectionAssert.AreEqual(transferCoinsTransaction.Fees, readTransferCoinsTransaction.Fees);
            Assert.AreEqual(transferCoinsTransaction.ReceivedCoins.ToJson(), readTransferCoinsTransaction.ReceivedCoins.ToJson());
            CollectionAssert.AreEqual(transferCoinsTransaction.SpentCoins, readTransferCoinsTransaction.SpentCoins);
            
            Assert.AreEqual(transaction.ToJson(), readTransaction.ToJson());
        }

        [Test]
        public async Task CanSaveAndRetrieveTransferAmount()
        {
            var repo = BuildRepo();

            var transaction = BuildRandomTransferAmountExecutedTransaction();
            var transferAmountTransaction = transaction.AsTransferAmount();

            await repo.AddIfNotExistsAsync(new[] {transaction});

            var readTransaction = await repo.GetAsync(transaction.BlockchainType, transferAmountTransaction.TransactionId);

            Assert.IsTrue(readTransaction.IsTransferAmount);

            var readTransferAmountTransaction = readTransaction.AsTransferAmount();

            Assert.AreEqual(transaction.BlockId, readTransaction.BlockId);
            Assert.AreEqual(transferAmountTransaction.TransactionId, readTransferAmountTransaction.TransactionId);
            CollectionAssert.AreEqual(transferAmountTransaction.Fees, readTransferAmountTransaction.Fees);
            Assert.AreEqual(transferAmountTransaction.BalanceChanges.ToJson(), readTransferAmountTransaction.BalanceChanges.ToJson());
            Assert.AreEqual(transferAmountTransaction.IsIrreversible, readTransferAmountTransaction.IsIrreversible);
            
            Assert.AreEqual(transaction.ToJson(), readTransaction.ToJson());
        }

        [Test]
        public async Task CanSaveAndRetrieveFailedEvent()
        {
            var repo = BuildRepo();

            var transaction = BuildRandomFailedTransaction();
            var failedTransaction = transaction.AsFailed();

            await repo.AddIfNotExistsAsync(new[] {transaction});

            var readTransaction = await repo.GetAsync(transaction.BlockchainType, failedTransaction.TransactionId);

            Assert.IsTrue(readTransaction.IsFailed);
            Assert.AreEqual(transaction.ToJson(), readTransaction.ToJson());
        }
        
        [Test]
        public async Task DoNotUpdates()
        {
            var repo = BuildRepo();

            var transaction1 = BuildRandomTransferCoinsExecutedTransaction();
            var transferCoinsTransaction = transaction1.AsTransferCoins();
            var transaction2 = BuildRandomTransferCoinsExecutedTransaction(transaction1.BlockId, transferCoinsTransaction.TransactionId);

            await repo.AddIfNotExistsAsync(new[] {transaction1});
            await repo.AddIfNotExistsAsync(new[] {transaction2});

            var readTransaction = await repo.GetAsync(transaction1.BlockchainType, transferCoinsTransaction.TransactionId);

            Assert.IsTrue(readTransaction.IsTransferCoins);

            var readTransferCoinsTransaction = readTransaction.AsTransferCoins();

            Assert.AreEqual(transaction1.BlockId, readTransaction.BlockId);
            CollectionAssert.AreEqual(transferCoinsTransaction.Fees, readTransferCoinsTransaction.Fees);
            Assert.AreEqual(transferCoinsTransaction.ReceivedCoins.ToJson(), readTransferCoinsTransaction.ReceivedCoins.ToJson());
            Assert.AreEqual(transferCoinsTransaction.SpentCoins.ToJson(), readTransferCoinsTransaction.SpentCoins.ToJson());

            Assert.AreEqual(transaction1.ToJson(), readTransaction.ToJson());
        }

        [Test]
        public async Task CanDelete()
        {
            var repo = BuildRepo();

            var transaction = BuildRandomTransferCoinsExecutedTransaction();
            var transferCoinsTransaction = transaction.AsTransferCoins();

            await repo.AddIfNotExistsAsync(new[] {transaction});

            var readTransaction1 = await repo.GetOrDefaultAsync(transaction.BlockchainType, transferCoinsTransaction.TransactionId);
            
            Assert.IsNotNull(readTransaction1);
            Assert.IsTrue(readTransaction1.IsTransferCoins);

            await repo.TryRemoveAllOfBlockAsync(transaction.BlockchainType, transaction.BlockId);

            var readTransaction2 = await repo.GetOrDefaultAsync(transaction.BlockchainType, transferCoinsTransaction.TransactionId);
            
            Assert.IsNull(readTransaction2);
        }

        [Test]
        public async Task CanSanitazePg()
        {
            var repo = BuildRepo();

            var transaction = BuildRandomTransferAmountExecutedTransaction(assetId: "\u0000\u0000\u0000\u0000\u0000\u0000");
            var transferAmountTx = transaction.AsTransferAmount();

            await repo.AddIfNotExistsAsync(new[] { transaction });

            var readTransaction1 = await repo.GetOrDefaultAsync(transaction.BlockchainType, transferAmountTx.TransactionId);

            Assert.IsNotNull(readTransaction1);
            Assert.IsTrue(readTransaction1.IsTransferAmount);

            await repo.TryRemoveAllOfBlockAsync(transaction.BlockchainType, transaction.BlockId);

            var readTransaction2 = await repo.GetOrDefaultAsync(transaction.BlockchainType, transferAmountTx.TransactionId);

            Assert.IsNull(readTransaction2);
        }

        [Test]
        public async Task CanCalcCount()
        {
            var repo = BuildRepo();

            var blockchainType = Guid.NewGuid().ToString();
            var blockId = Guid.NewGuid().ToString();

            var transactions = Enumerable.Range(0, 9)
                .Select(i => BuildRandomTransferCoinsExecutedTransaction(blockchainType: blockchainType, blockId: blockId))
                .ToArray();

            await repo.AddIfNotExistsAsync(transactions);

            var counted = await repo.CountInBlockAsync(blockchainType, blockId);

            Assert.AreEqual(counted, transactions.Length);
        }
        
        private static Transaction BuildRandomTransferCoinsExecutedTransaction(string blockchainType = null, TransactionId transactionId = null, string blockId = null)
        {
            var rnd = new Random();

            return new Transaction
            (
                blockchainType ?? Guid.NewGuid().ToString(),
                blockId ?? Guid.NewGuid().ToString(),
                new TransferCoinsExecutedTransaction
                (
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
                    }
                )
            );
        }

        private static Transaction BuildRandomTransferAmountExecutedTransaction(string assetId = null)
        {
            var rdm = new Random();

            return new Transaction
            (
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),

                new TransferAmountExecutedTransaction
                (
                    rdm.Next(),
                    new TransactionId(Guid.NewGuid().ToString()),
                    new[]
                    {
                        BuildRandomBalanceChange(assetId),
                        BuildRandomBalanceChange(assetId)
                    },
                    new[]
                    {
                        BuildRandomFee(),
                        BuildRandomFee(),
                        BuildRandomFee()
                    }
                )
            );
        }

        private static Transaction BuildRandomFailedTransaction()
        {
            var rdm = new Random();

            return new Transaction
            (
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                new FailedTransaction
                (
                    rdm.Next(),
                    new TransactionId(Guid.NewGuid().ToString()),
                    TransactionBroadcastingError.NotEnoughBalance,
                    Guid.NewGuid().ToString(),
                    new[]
                    {
                        BuildRandomFee()
                    }
                )
            );
        }

        private static BalanceChange BuildRandomBalanceChange(string assetId = null)
        {
            var rdm = new Random();
            return new BalanceChange(Guid.NewGuid().ToString(), new Asset(new AssetId(assetId ?? Guid.NewGuid().ToString())), new Money(rdm.Next(), 0), new Address(Guid.NewGuid().ToString()), new AddressTag(Guid.NewGuid().ToString()), AddressTagType.Number, rdm.Next());
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
            return new TransactionsRepository(ContextFactory.GetPosgresTestsConnStringProvider());
        }
    }
}
