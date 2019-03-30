//using System.Linq;
//using Lykke.Job.Bil2Indexer.DomainServices;
//using Lykke.Job.Bil2Indexer.Tests.Mocks;
//using NUnit.Framework;

//namespace Lykke.Job.Bil2Indexer.Tests
//{
//    [TestFixture]
//    public class BlocksBuilderTests
//    {
//        [Test]
//        public void Test()
//        {
//            // TODO:
//            // 1. When chainCrawler has been added a block, it should call blocksBuilder which saves a blockBuilding.
//            // 2. New blockBuilding should be added to the blocks being built list. blocksBuilder keeps this list in memory.
//            // 3. blocksBuilder iterates list of the blocks being built in the background thread and updates number of the stored transactions.
//            // 4. blocksBuilder another background thread with more iteration period reads blocks being built from the storage to the in memory list (it only adds items to the list).
//            // 5. When transaction event is received, a record should be added to the accounts storage:
//            //      - address, blockNumber, blockId, transactionId, amount, balance
//            // 6. If there was no such a record ^ or it was different, then right after adding, balances of all records with more recent blocks and related to the address should be updated.
//            //    or it could be done in the background thread.

//            // Arrange

//            var blocksBuildingsRepository = new InMemoryBlockFlagRepository();
//            //var blocksGatherer = new BlocksBuilder(blocksBuildingsRepository, TODO, TODO);
            
//            var blockId = "block_id";
//            var transactionId = "tx_id";

//            // Act

//            // Assert

//            var blockBuildings = blocksBuildingsRepository.GetAll();

//            Assert.AreEqual(1, blockBuildings.Count);

//            var blockBuilding = blockBuildings.Single();

//            Assert.AreEqual(blockId, blockBuilding.Id);
//            Assert.AreEqual(1, blockBuilding.ReceivedTransactionsNumber);
//            Assert.IsNull(blockBuilding.TotalTransactionsNumber);
//        }
//    }
//}
