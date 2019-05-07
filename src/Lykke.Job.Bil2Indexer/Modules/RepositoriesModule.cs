using System.Linq;
using Autofac;
using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.Decorators.AppInsight;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Settings;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BalanceActions;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BlockHeaders;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.ChainHeads;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Crawlers;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions;
using Lykke.SettingsReader;

namespace Lykke.Job.Bil2Indexer.Modules
{
    [UsedImplicitly]
    public class RepositoriesModule : Module
    {
        private readonly AppSettings _settings;

        public RepositoriesModule(IReloadingManager<AppSettings> settings)
        {
            _settings = settings.CurrentValue;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var blockchainConnStringProvider = new PgConnectionStringProvider(
                _settings.Bil2IndexerJob.BlockchainIntegrations.ToDictionary(p => p.Type,
                    p => p.Indexer.PgBlockchainDataConnString));

            var stateConnStringProvider = new PgConnectionStringProvider(
                _settings.Bil2IndexerJob.BlockchainIntegrations.ToDictionary(p => p.Type,
                    p => p.Indexer.PgStateDataConnString));

            var transactionConnStringProvider = new PgConnectionStringProvider(
                _settings.Bil2IndexerJob.BlockchainIntegrations.ToDictionary(p => p.Type,
                    p => p.Indexer.PgTransactionsDataConnString));

            builder.RegisterType<BalanceActionsRepository>()
                .As<IBalanceActionsRepository>()
                .WithParameter(TypedParameter.From(blockchainConnStringProvider))
                .SingleInstance();

            builder.RegisterType<BlockHeadersRepository>()
                .As<IBlockHeadersRepository>()
                .WithParameter(TypedParameter.From(stateConnStringProvider))
                .SingleInstance();

            builder.RegisterType<CoinsRepository>()
                .As<ICoinsRepository>()
                .WithParameter(TypedParameter.From(blockchainConnStringProvider))
                .SingleInstance();

            builder.RegisterType<CrawlersRepository>()
                .As<ICrawlersRepository>()
                .WithParameter(TypedParameter.From(stateConnStringProvider))
                .SingleInstance();

            builder.RegisterType<TransactionsRepository>()
                .As<ITransactionsRepository>()
                .WithParameter(TypedParameter.From(transactionConnStringProvider))
                .SingleInstance();

            builder.RegisterType<ChainHeadsRepository>()
                .As<IChainHeadsRepository>()
                .WithParameter(TypedParameter.From(stateConnStringProvider))
                .SingleInstance();

            builder.RegisterType<FeeEnvelopesRepository>()
                .As<IFeeEnvelopesRepository>()
                .WithParameter(TypedParameter.From(blockchainConnStringProvider))
                .SingleInstance();

            builder.RegisterType<AssetInfosRepository>()
                .As<IAssetInfosRepository>()
                .WithParameter(TypedParameter.From(blockchainConnStringProvider))
                .SingleInstance();

            #region Decorators

            builder.RegisterDecorator<BalanceActionsRepositoryAppInsightDecorator, IBalanceActionsRepository>();
            builder.RegisterDecorator<BlockHeadersRepositoryAppInsightDecorator, IBlockHeadersRepository>();
            builder.RegisterDecorator<CoinsRepositoryAppInsightDecorator, ICoinsRepository>();
            builder.RegisterDecorator<CrawlersRepositoryAppInsightDecorator, ICrawlersRepository>();
            builder.RegisterDecorator<TransactionsRepositoryAppInsightDecorator, ITransactionsRepository>();
            builder.RegisterDecorator<ChainHeadsRepositoryAppInsightDecorator, IChainHeadsRepository>();
            builder.RegisterDecorator<FeeEnvelopesRepositoryAppInsightDecorator, IFeeEnvelopesRepository>();
            builder.RegisterDecorator<AssetInfosRepositoryAppInsightDecorator, IAssetInfosRepository>();
            
            #endregion
        }
    }
}
