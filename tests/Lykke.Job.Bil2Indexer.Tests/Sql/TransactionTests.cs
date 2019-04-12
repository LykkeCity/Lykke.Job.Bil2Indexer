using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.Contract.Common;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions;
using Lykke.Logs;
using Lykke.Numerics;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture]
    public class TransactionTests
    {
        [Test]
        public async Task CanSave()
        {
            var repo = BuildRepo();

            var entity = BuildRandomTransferCoinsTransactionExecutedEvent();

            await repo.SaveAsync("blockchain-type", entity);
            await repo.SaveAsync("blockchain-type", entity);
        }

        private TransferCoinsTransactionExecutedEvent BuildRandomTransferCoinsTransactionExecutedEvent()
        {
            var rnd = new Random();

            return new TransferCoinsTransactionExecutedEvent(
                Guid.NewGuid().ToString(),
                rnd.Next(), Guid.NewGuid().ToString(),
                new List<ReceivedCoin>
                {
                    BuildRandmonReceivedCoin(),
                    BuildRandmonReceivedCoin(),
                    BuildRandmonReceivedCoin()

                }, new List<CoinReference>()
                {
                    BuildRandmomSpentCoin(),
                    BuildRandmomSpentCoin(),
                    BuildRandmomSpentCoin(),
                    BuildRandmomSpentCoin(),
                    BuildRandmomSpentCoin()
                }, new Fee[0]);
        }

        private ReceivedCoin BuildRandmonReceivedCoin()
        {
            var rnd = new Random();
            return new ReceivedCoin(rnd.Next(), 
                new Asset(new AssetId(Guid.NewGuid().ToString())),
                new UMoney(new BigInteger(rnd.Next()), 12));
        }

        private CoinReference BuildRandmomSpentCoin()
        {
            var rnd = new Random();

            return new CoinReference(Guid.NewGuid().ToString(), rnd.Next());
        }
        private TransactionsRepository BuildRepo()
        {
            return new TransactionsRepository(ContextFactory.GetPosgresTestsConnString(), EmptyLogFactory.Instance);
        }
    }
}
