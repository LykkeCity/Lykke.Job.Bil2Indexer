using System.Threading.Tasks;
using Hangfire;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Workflow.BackgroundJobs;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class BlockEventsHandler : IBlockEventsHandler
    {
        private readonly IntegrationSettingsProvider _integrationSettingsProvider;
        private readonly IChainCrawlersManager _chainCrawlersManager;

        public BlockEventsHandler(
            IntegrationSettingsProvider integrationSettingsProvider,
            IChainCrawlersManager chainCrawlersManager)
        {
            _integrationSettingsProvider = integrationSettingsProvider;
            _chainCrawlersManager = chainCrawlersManager;
        }

        public Task HandleAsync(string blockchainType, BlockHeaderReadEvent evt, IMessagePublisher publisher)
        {
            var blockHeader = new BlockHeader
            (
                evt.BlockNumber,
                evt.BlockId,
                evt.BlockMiningMoment,
                evt.BlockSize,
                evt.BlockTransactionsCount,
                evt.PreviousBlockId
            );

            return _chainCrawlersManager.ProcessBlockAsync(blockchainType, blockHeader);
        }

        public Task HandleAsync(string blockchainType, BlockNotFoundEvent evt, IMessagePublisher publisher)
        {
            var delay = _integrationSettingsProvider.Get(blockchainType).Indexer.NotFoundBlockRetryDelay;

            BackgroundJob.Schedule<RetryNotFoundBlockBackgroundJob>
            (
                job => job.RetryAsync(blockchainType, evt.BlockNumber),
                delay
            );

            return Task.CompletedTask;
        }

        public Task HandleAsync(string blockchainType, TransferAmountTransactionExecutedEvent evt, IMessagePublisher publisher)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(string blockchainType, TransferCoinsTransactionExecutedEvent evt, IMessagePublisher publisher)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(string blockchainType, TransactionFailedEvent evt, IMessagePublisher publisher)
        {
            return Task.CompletedTask;
        }

        public Task HandleAsync(string blockchainType, LastIrreversibleBlockUpdatedEvent evt, IMessagePublisher publisher)
        {
            return Task.CompletedTask;
        }
    }
}
