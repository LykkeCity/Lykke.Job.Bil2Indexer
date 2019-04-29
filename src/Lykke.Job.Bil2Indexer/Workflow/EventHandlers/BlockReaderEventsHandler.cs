using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Numerics.Linq;

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
        private readonly IBlocksReaderApiFactory _blocksReaderApiFactory;
        private readonly ILog _log;

        public BlockReaderEventsHandler(
            ILogFactory logFactory,
            IMessageSendersFactory messageSendersFactory,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider integrationSettingsProvider,
            ICrawlersManager crawlersManager,
            ITransactionsRepository transactionsRepository,
            IBalanceActionsRepository balanceActionsRepository,
            ICoinsRepository coinsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            IAssetInfosManager assetInfosManager,
            IBlocksReaderApiFactory blocksReaderApiFactory)
        {
            _log = logFactory.CreateLog(this);
            _messageSendersFactory = messageSendersFactory;
            _blockHeadersRepository = blockHeadersRepository;
            _integrationSettingsProvider = integrationSettingsProvider;
            _crawlersManager = crawlersManager;
            _transactionsRepository = transactionsRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _coinsRepository = coinsRepository;
            _feeEnvelopesRepository = feeEnvelopesRepository;
            _assetInfosManager = assetInfosManager;
            _blocksReaderApiFactory = blocksReaderApiFactory;
        }

        public async Task<MessageHandlingResult> HandleAsync(string blockchainType, BlockHeaderReadEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, evt.BlockNumber);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(crawlerCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                _log.LogLegacyMessage(evt, headers);

                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(crawlerCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            var blockHeader = await _blockHeadersRepository.GetOrDefaultAsync(blockchainType, evt.BlockId)
                              ?? BlockHeader.StartAssembling
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

            return MessageHandlingResult.Success();
        }

        public async Task<MessageHandlingResult> HandleAsync(string blockchainType, BlockNotFoundEvent evt, MessageHeaders headers, IMessagePublisher responsePublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, evt.BlockNumber);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(crawlerCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(crawlerCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            await Task.Delay(_integrationSettingsProvider.Get(blockchainType).Indexer.NotFoundBlockRetryDelay);
         
            var blocksReaderApi = _blocksReaderApiFactory.Create(blockchainType);

            await blocksReaderApi.SendAsync(new ReadBlockCommand(evt.BlockNumber), crawler.GetCorrelationId().ToString());

            return MessageHandlingResult.Success();
        }

        public async Task<MessageHandlingResult> HandleAsync(string blockchainType, TransferAmountTransactionExecutedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, messageCorrelationId.Configuration);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(crawlerCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(crawlerCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            var saveTransactionTask = _transactionsRepository.AddIfNotExistsAsync(blockchainType, evt);

            var actions = evt.BalanceChanges
                .Where(x => x.Address != null)
                .GroupBy(x => new {x.Address, x.Asset})
                .Select
                (
                    g => new BalanceAction
                    (
                        new AccountId(g.Key.Address, g.Key.Asset),
                        g.Sum(x => x.Value),
                        crawler.ExpectedBlockNumber,
                        evt.BlockId,
                        evt.TransactionId
                    )
                )
                .ToArray();

            var saveBalanceActionsTask = _balanceActionsRepository.AddIfNotExistsAsync(blockchainType, actions);

            var assetInfos = evt.BalanceChanges
                .Select(x => new AssetInfo(blockchainType, x.Asset, x.Value.Scale))
                .ToHashSet();

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

            return MessageHandlingResult.Success();
        }

        public async Task<MessageHandlingResult> HandleAsync(string blockchainType, TransferCoinsTransactionExecutedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            if (evt.Fees != null)
            {
                throw new NotSupportedException();
            }

            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, messageCorrelationId.Configuration);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(crawlerCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(crawlerCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
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

            var assetInfos = evt.ReceivedCoins
                .Select(x => new AssetInfo(blockchainType, x.Asset, x.Value.Scale))
                .ToHashSet();

            var saveAssetInfosTask = _assetInfosManager.EnsureAdded(assetInfos);

            await Task.WhenAll
            (
                saveTransactionTask,
                saveCoinsTask,
                saveAssetInfosTask
            );

            return MessageHandlingResult.Success();
        }

        public async Task<MessageHandlingResult> HandleAsync(string blockchainType, TransactionFailedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);
            var crawler = await _crawlersManager.GetCrawlerAsync(blockchainType, messageCorrelationId.Configuration);
            var crawlerCorrelationId = crawler.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(crawlerCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                return MessageHandlingResult.Success();
            }

            if (messageCorrelationId.IsPrematureRelativeTo(crawlerCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
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

            return MessageHandlingResult.Success();
        }

        public Task<MessageHandlingResult> HandleAsync(string blockchainType, LastIrreversibleBlockUpdatedEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
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

            return Task.FromResult(MessageHandlingResult.Success());
        }
    }
}
