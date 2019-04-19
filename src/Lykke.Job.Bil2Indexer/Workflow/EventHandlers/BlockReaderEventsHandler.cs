using System;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Workflow.BackgroundJobs;
using Lykke.Job.Bil2Indexer.Workflow.Commands;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class BlockReaderEventsHandler : IBlockEventsHandler
    {
        private readonly IMessageSendersFactory _messageSendersFactory;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IntegrationSettingsProvider _integrationSettingsProvider;
        private readonly ICrawlersManager _crawlersManager;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly ICoinsRepository _coinsRepository;
        private readonly IFeeEnvelopesRepository _feeEnvelopesRepository;
        private readonly IAssetInfosManager _assetInfosManager;

        public BlockReaderEventsHandler(
            IMessageSendersFactory messageSendersFactory,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider integrationSettingsProvider,
            ICrawlersManager crawlersManager,
            ITransactionsRepository transactionsRepository,
            IBalanceActionsRepository balanceActionsRepository,
            ICoinsRepository coinsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            IAssetInfosManager assetInfosManager)
        {
            _messageSendersFactory = messageSendersFactory;
            _blockHeadersRepository = blockHeadersRepository;
            _integrationSettingsProvider = integrationSettingsProvider;
            _crawlersManager = crawlersManager;
            _transactionsRepository = transactionsRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _coinsRepository = coinsRepository;
            _feeEnvelopesRepository = feeEnvelopesRepository;
            _assetInfosManager = assetInfosManager;
        }

        public async Task HandleAsync(string blockchainType, BlockHeaderReadEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, evt.BlockNumber);

            if (!crawler.GetCorrelationId().Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return;
            }

            var blockHeader = BlockHeader.StartAssembling
            (
                evt.BlockId,
                blockchainType,
                evt.BlockNumber,
                evt.BlockMiningMoment,
                evt.BlockSize, evt.BlockTransactionsCount, evt.PreviousBlockId
            );
            
            var commandsSender = _messageSendersFactory.CreateCommandsSender();

            await _blockHeadersRepository.SaveAsync(blockHeader);

            commandsSender.Publish
            (
                new WaitForBlockAssemblingCommand
                {
                    BlockchainType = blockchainType,
                    BlockId = evt.BlockId
                },
                headers.CorrelationId
            );
        }

        public async Task HandleAsync(string blockchainType, BlockNotFoundEvent evt, MessageHeaders headers, IMessagePublisher responsePublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, evt.BlockNumber);

            if (!crawler.GetCorrelationId().Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return;
            }

            // TODO: if delay is less than some configured threshold, use Task.Delay instead of scheduler,
            // because of too high latency of the scheduler.

            var delay = _integrationSettingsProvider.Get(blockchainType).Indexer.NotFoundBlockRetryDelay;

            BackgroundJob.Schedule<RetryNotFoundBlockJob>
            (
                job => job.RetryAsync(blockchainType, evt.BlockNumber, messageCorrelationId),
                delay
            );
        }

        public async Task HandleAsync(string blockchainType, TransferAmountTransactionExecutedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, messageCorrelationId.Configuration);

            if (!crawler.GetCorrelationId().Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return;
            }

            var saveTransactionTask = _transactionsRepository.AddIfNotExistsAsync(blockchainType, evt);

            var actions = evt.BalanceChanges
                .Where(x => x.Address != null)
                .Select
                (
                    x => new BalanceAction
                    (
                        new AccountId(x.Address, x.Asset),
                        x.Value,
                        crawler.ExpectedBlockNumber,
                        evt.BlockId,
                        evt.TransactionId
                    )
                )
                .ToArray();

            var saveBalanceActionsTask = _balanceActionsRepository.AddIfNotExistsAsync(blockchainType, actions);

            var assetInfos = evt.BalanceChanges
                .Select(x => new AssetInfo(blockchainType, x.Asset, x.Value.Scale))
                .ToArray();

            var saveAssetInfosTask = _assetInfosManager.EnsureAdded(assetInfos);

            var fees = evt.Fees
                .Select
                (
                    x => new FeeEnvelope
                    (
                        blockchainType,
                        evt.BlockId,
                        evt.TransactionId,
                        x
                    )
                )
                .ToArray();

            await Task.WhenAll
            (
                saveTransactionTask,
                saveBalanceActionsTask,
                saveAssetInfosTask,
                _feeEnvelopesRepository.AddIfNotExistsAsync(fees)
            );
        }

        public async Task HandleAsync(string blockchainType, TransferCoinsTransactionExecutedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            if (evt.Fees != null)
            {
                throw new NotSupportedException();
            }

            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, messageCorrelationId.Configuration);

            if (!crawler.GetCorrelationId().Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return;
            }

            var saveTransactionTask = _transactionsRepository.AddIfNotExistsAsync(blockchainType, evt);

            var coins = evt.ReceivedCoins
                .Select
                (
                    x => Coin.CreateUnspent
                    (
                        blockchainType,
                        new CoinId(evt.TransactionId, x.CoinNumber),
                        x.Asset,
                        x.Value,
                        x.Address,
                        x.AddressTag,
                        x.AddressTagType,
                        x.AddressNonce
                    )
                )
                .ToArray();

            var saveCoinsTask = _coinsRepository.AddIfNotExistsAsync(coins);

            var actions = evt.ReceivedCoins
                .Where(c => c.Address != null)
                .Select
                (
                    x => new BalanceAction
                    (
                        new AccountId(x.Address, x.Asset),
                        x.Value,
                        crawler.ExpectedBlockNumber,
                        evt.BlockId,
                        evt.TransactionId
                    )
                )
                .ToArray();

            var assetInfos = evt.ReceivedCoins
                .Select(x => new AssetInfo(blockchainType, x.Asset, x.Value.Scale))
                .ToArray();

            var saveAssetInfosTask = _assetInfosManager.EnsureAdded(assetInfos);

            await Task.WhenAll
            (
                saveTransactionTask,
                saveCoinsTask,
                saveAssetInfosTask,
                _balanceActionsRepository.AddIfNotExistsAsync(blockchainType, actions)
            );
        }

        public async Task HandleAsync(string blockchainType, TransactionFailedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, messageCorrelationId.Configuration);

            if (!crawler.GetCorrelationId().Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return;
            }
            
            var saveTransactionTask = _transactionsRepository.AddIfNotExistsAsync(blockchainType, evt);

            var fees = evt.Fees
                .Select
                (
                    x => new FeeEnvelope
                    (
                        blockchainType,
                        evt.BlockId,
                        evt.TransactionId,
                        x
                    )
                )
                .ToArray();

            await Task.WhenAll
            (
                saveTransactionTask,
                _feeEnvelopesRepository.AddIfNotExistsAsync(fees)
            );
        }

        public Task HandleAsync(string blockchainType, LastIrreversibleBlockUpdatedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var eventsPublisher = _messageSendersFactory.CreateEventsPublisher();

            eventsPublisher.Publish
            (
                new Contract.Events.LastIrreversibleBlockUpdatedEvent
                (
                    blockchainType,
                    evt.BlockNumber,
                    evt.BlockId
                ),
                headers.CorrelationId
            );

            return Task.CompletedTask;
        }
    }
}
