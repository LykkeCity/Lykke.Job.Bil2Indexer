using Autofac;
using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.Decorators;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Settings;
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
            builder.RegisterType<BalanceActionsRepository>()
                .Named<IBalanceActionsRepository>("original")
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.Db.PgBlockchainDataConnString))
                .SingleInstance();

            builder.RegisterType<BlockHeadersRepository>()
                .Named<IBlockHeadersRepository>("original")
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.Db.PgStateDataConnString))
                .SingleInstance();

            builder.RegisterType<CoinsRepository>()
                .Named<ICoinsRepository>("original")
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.Db.PgBlockchainDataConnString))
                .SingleInstance();

            builder.RegisterType<CrawlersRepository>()
                .Named<ICrawlersRepository>("original")
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.Db.PgStateDataConnString))
                .SingleInstance();

            builder.RegisterType<TransactionsRepository>()
                .Named<ITransactionsRepository>("original")
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.Db.PgTransactionsDataConnString))
                .SingleInstance();

            builder.RegisterType<ChainHeadsRepository>()
                .Named<IChainHeadsRepository>("original")
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.Db.PgStateDataConnString))
                .SingleInstance();

            builder.RegisterType<FeeEnvelopesRepository>()
                .Named<IFeeEnvelopesRepository>("original")
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.Db.PgBlockchainDataConnString))
                .SingleInstance();

            builder.RegisterType<AssetInfosRepository>()
                .Named<IAssetInfosRepository>("original")
                .WithParameter(TypedParameter.From(_settings.Bil2IndexerJob.Db.PgBlockchainDataConnString))
                .SingleInstance();

            #region Decorators

            builder.RegisterType<AppInsightTelemetryProvider>()
                .As<IAppInsightTelemetryProvider>();


            builder.RegisterType<BalanceActionsDecoratorRepository>()
                .Named<IBalanceActionsRepository>("decorator")
                .SingleInstance();

            builder.RegisterType<BlockHeadersDecoratorRepository>()
                .Named<IBlockHeadersRepository>("decorator")
                .SingleInstance();

            builder.RegisterType<CoinsDecoratorRepository>()
                .Named<ICoinsRepository>("decorator")
                .SingleInstance();

            builder.RegisterType<CrawlersDecoratorRepository>()
                .Named<ICrawlersRepository>("decorator")
                .SingleInstance();

            builder.RegisterType<TransactionsDecoratorRepository>()
                .Named<ITransactionsRepository>("decorator")
                .SingleInstance();

            builder.RegisterType<ChainHeadsDecoratorRepository>()
                .Named<IChainHeadsRepository>("decorator")
                .SingleInstance();

            builder.RegisterType<FeeEnvelopesDecoratorRepository>()
                .Named<IFeeEnvelopesRepository>("decorator")
                .SingleInstance();

            builder.RegisterType<AssetInfosDecoratorRepository>()
                .Named<IAssetInfosRepository>("decorator")
                .SingleInstance();


            builder.RegisterDecorator<IAssetInfosRepository>((c, inner) => 
                    c.ResolveNamed<IAssetInfosRepository>("decorator", TypedParameter.From(inner)), "original")
                .As<IAssetInfosRepository>();

            builder.RegisterDecorator<IBalanceActionsRepository>((c, inner) =>
                    c.ResolveNamed<IBalanceActionsRepository>("decorator", TypedParameter.From(inner)), "original")
                .As<IBalanceActionsRepository>();

            builder.RegisterDecorator<IBlockHeadersRepository>((c, inner) =>
                    c.ResolveNamed<IBlockHeadersRepository>("decorator", TypedParameter.From(inner)), "original")
                .As<IBlockHeadersRepository>();

            builder.RegisterDecorator<ICoinsRepository>((c, inner) =>
                    c.ResolveNamed<ICoinsRepository>("decorator", TypedParameter.From(inner)), "original")
                .As<ICoinsRepository>();

            builder.RegisterDecorator<ICrawlersRepository>((c, inner) =>
                    c.ResolveNamed<ICrawlersRepository>("decorator", TypedParameter.From(inner)), "original")
                .As<ICrawlersRepository>();

            builder.RegisterDecorator<ITransactionsRepository>((c, inner) =>
                    c.ResolveNamed<ITransactionsRepository>("decorator", TypedParameter.From(inner)), "original")
                .As<ITransactionsRepository>();

            builder.RegisterDecorator<IChainHeadsRepository>((c, inner) =>
                    c.ResolveNamed<IChainHeadsRepository>("decorator", TypedParameter.From(inner)), "original")
                .As<IChainHeadsRepository>();

            builder.RegisterDecorator<IFeeEnvelopesRepository>((c, inner) =>
                    c.ResolveNamed<IFeeEnvelopesRepository>("decorator", TypedParameter.From(inner)), "original")
                .As<IFeeEnvelopesRepository>();

            builder.RegisterDecorator<IAssetInfosRepository>((c, inner) =>
                    c.ResolveNamed<IAssetInfosRepository>("decorator", TypedParameter.From(inner)), "original")
                .As<IAssetInfosRepository>();


            #endregion
        }
    }
}
