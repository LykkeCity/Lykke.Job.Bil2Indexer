using System;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Dapper;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
using Npgsql;
using NUnit.Framework;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture()]
    class AssetInfosTests
    {
        [Test]
        public async Task CanSaveAndRead()
        {
            var repo = new AssetInfosRepository(ContextFactory.GetPosgresTestsConnStringProvider());

            var asset = BuildRandomAssetInfo();

            var assets = new[]
            {
                asset,
                asset,
                BuildRandomAssetInfo(),
                BuildRandomAssetInfo(),
                BuildRandomAssetInfo(),
                BuildRandomAssetInfo(),
                BuildRandomAssetInfo()
            }.OrderBy(p=>p.Asset.Id).ToList();

            await repo.AddIfNotExistsAsync(assets);
            await repo.AddIfNotExistsAsync(assets);
            await repo.AddIfNotExistsAsync(assets);

            var retrieved1 = await repo.GetOrDefaultAsync(asset.BlockchainType, asset.Asset);
            var retrieved2 = await repo.GetAsync(asset.BlockchainType, asset.Asset);
            var retrieved3 = (await repo.GetSomeOfAsync(asset.BlockchainType, new []{ asset.Asset }))
                .Single();

            var retrieved4 = await repo.GetAllAsync(asset.BlockchainType, 3, true, null,
                null);
            
            Assert.AreEqual(3, retrieved4.Count);

            AssertEquals(retrieved1, asset);
            AssertEquals(retrieved2, asset);
            AssertEquals(retrieved3, asset);
        }
        [Test]
        public async Task CanFilter()
        {
            RemoveAll();

            var repo = new AssetInfosRepository(ContextFactory.GetPosgresTestsConnStringProvider());

            var bType = Guid.NewGuid().ToString();
            
            var assets = new[]
            {
                new AssetInfo(bType, new Asset(new AssetId("aaa")), 13),
                new AssetInfo(bType, new Asset(new AssetId("bbb")), 12),
                new AssetInfo(bType, new Asset(new AssetId("ccc")), 13),
                new AssetInfo(bType, new Asset(new AssetId("ddd")), 14),
                new AssetInfo(bType, new Asset(new AssetId("eee")), 14),
                new AssetInfo(bType, new Asset(new AssetId("fff")), 14),
                new AssetInfo(bType, new Asset(new AssetId("xxx")), 14),
            }.OrderBy(p => p.Asset.Id).ToList();

            await repo.AddIfNotExistsAsync(assets);

            var retrieved1 = (await repo.GetAllAsync(bType, 9999, true, startingAfter: "bbb", endingBefore: "fff")).ToArray();

            Assert.AreEqual(3, retrieved1.Length);

            var index = 0;
            foreach (var asset in assets.Skip(2).Take(3).OrderBy(p=>p.Asset.Id.ToString()))
            {
                Assert.AreEqual(asset.Asset, retrieved1[index].Asset);
                AssertEquals(asset, retrieved1[index]);
                index++;
            }

            var retrieved2 = (await repo.GetAllAsync(bType, 9999, false, startingAfter: "ccc", endingBefore: "xxx")).ToArray();
            index = 0;
            foreach (var asset in assets.Skip(3).Take(3).OrderByDescending(p => p.Asset.Id.ToString()))
            {
                Assert.AreEqual(asset.Asset, retrieved2[index].Asset);
                AssertEquals(asset, retrieved2[index]);
                index++;
            }
        }

        private void RemoveAll()
        {
            using (var conn = new NpgsqlConnection(ContextFactory.GetPosgresTestsConnString()))
            {
                conn.Execute("truncate table assets");
            }
        }


        private AssetInfo BuildRandomAssetInfo()
        {
            return new AssetInfo(Guid.NewGuid().ToString("N"), new Asset(Guid.NewGuid().ToString("N")), new Random().Next() );
        }


        private void AssertEquals(AssetInfo a, AssetInfo b)
        {
            Assert.AreEqual(a.ToJson(), b.ToJson());
        }

    }
}
