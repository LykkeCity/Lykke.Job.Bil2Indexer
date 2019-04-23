﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos;
using Lykke.Job.Bil2Indexer.Tests.Sql.Mocks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Lykke.Job.Bil2Indexer.Tests.Sql
{
    [TestFixture()]
    class AssetInfosTests
    {
        [Test]
        public async Task CanSaveAndRead()
        {
            var repo = new AssetInfosRepository(ContextFactory.GetPosgresTestsConnString());

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
            };

            await repo.AddIfNotExistsAsync(assets);
            await repo.AddIfNotExistsAsync(assets);
            await repo.AddIfNotExistsAsync(assets);

            var retrieved1 = await repo.GetOrDefaultAsync(asset.BlockchainType, asset.Asset);
            var retrieved2 = await repo.GetAsync(asset.BlockchainType, asset.Asset);
            var retrieved3 = (await repo.GetSomeOfAsync(asset.BlockchainType, new []{ asset.Asset }))
                .Single();

            AssertEquals(retrieved1, asset);
            AssertEquals(retrieved2, asset);
            AssertEquals(retrieved3, asset);
        }

        private AssetInfo BuildRandomAssetInfo()
        {
            return new AssetInfo(Guid.NewGuid().ToString(), new Asset(Guid.NewGuid().ToString()), new Random().Next() );
        }


        private void AssertEquals(AssetInfo a, AssetInfo b)
        {
            Assert.AreEqual(a.BlockchainType, b.BlockchainType);
            Assert.AreEqual(a.Asset.Id, b.Asset.Id);
            Assert.AreEqual(a.Scale, b.Scale);
        }

    }
}
