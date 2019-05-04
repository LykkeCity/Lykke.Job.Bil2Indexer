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

        public async Task<MessageHandlingResult> HandleAsync(string blockchainType, TransferAmountTransactionsBatchEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
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

            var transactions = evt.TransferAmountExecutedTransactions
                .Select(x => new Transaction(blockchainType, evt.BlockId, x))
                .Concat
                (
                    evt.FailedTransactions
                        .Select(x => new Transaction(blockchainType, evt.BlockId, x))
                );
            var saveTransactionsTask = _transactionsRepository.AddIfNotExistsAsync(transactions);

            var transactionsBalanceChanges = evt.TransferAmountExecutedTransactions
                .SelectMany
                (
                    t => t.BalanceChanges.Select(b => new
                    {
                        Transaction = t,
                        BalanceChange = b
                    })
                )
                .ToArray();

            var balanceActions = transactionsBalanceChanges
                .Where(x => x.BalanceChange.Address != null)
                .GroupBy(x => new
                {
                    x.Transaction.TransactionId,
                    x.BalanceChange.Address,
                    x.BalanceChange.Asset
                })
                .Select
                (
                    g => new BalanceAction
                    (
                        new AccountId(g.Key.Address, g.Key.Asset),
                        g.Sum(x => x.BalanceChange.Value),
                        crawler.ExpectedBlockNumber,
                        evt.BlockId,
                        g.Key.TransactionId
                    )
                );

            var saveBalanceActionsTask = _balanceActionsRepository.AddIfNotExistsAsync(blockchainType, balanceActions);

            var assetInfos = transactionsBalanceChanges
                .Select(x => new AssetInfo(blockchainType, x.BalanceChange.Asset, x.BalanceChange.Value.Scale))
                .ToHashSet();

            var saveAssetInfosTask = _assetInfosManager.EnsureAdded(assetInfos);

            var fees = evt.TransferAmountExecutedTransactions
                .SelectMany
                (
                    t => t.Fees.Select(f => new
                    {
                        TransactionId = t.TransactionId,
                        Fee = f
                    })
                )
                .Concat
                (
                    evt.FailedTransactions
                        .Where(t => t.Fees != null)
                        .SelectMany
                        (
                            t => t.Fees.Select(f => new
                            {
                                TransactionId = t.TransactionId,
                                Fee = f
                            })
                        )
                )
                .Select
                (
                    x => new FeeEnvelope
                    (
                        blockchainType,
                        evt.BlockId,
                        x.TransactionId,
                        x.Fee
                    )
                );

            await Task.WhenAll
            (
                saveTransactionsTask,
                saveBalanceActionsTask,
                saveAssetInfosTask,
                _feeEnvelopesRepository.AddIfNotExistsAsync(fees)
            );

            return MessageHandlingResult.Success();
        }

        public async Task<MessageHandlingResult> HandleAsync(string blockchainType, TransferCoinsTransactionsBatchEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
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

            var transactions = evt.TransferCoinsExecutedTransactions
                .Select(x => new Transaction(blockchainType, evt.BlockId, x))
                .Concat
                (
                    evt.FailedTransactions
                        .Select(x => new Transaction(blockchainType, evt.BlockId, x))
                );
            var saveTransactionsTask = _transactionsRepository.AddIfNotExistsAsync(transactions);

            var transactionsReceivedCoins = evt.TransferCoinsExecutedTransactions
                .SelectMany
                (
                    t => t.ReceivedCoins.Select(c => new
                    {
                        Transaction = t,
                        Coin = c
                    })
                )
                .ToArray();

            var coins = transactionsReceivedCoins
                .Select
                (
                    x => Coin.CreateUnspent
                    (
                        blockchainType,
                        new CoinId(x.Transaction.TransactionId, x.Coin.CoinNumber),
                        x.Coin.Asset,
                        x.Coin.Value,
                        x.Coin.Address,
                        x.Coin.AddressTag,
                        x.Coin.AddressTagType,
                        x.Coin.AddressNonce
                    )
                );

            var saveCoinsTask = _coinsRepository.AddIfNotExistsAsync(coins);

            var assetInfos = transactionsReceivedCoins
                .Select(x => new AssetInfo(blockchainType, x.Coin.Asset, x.Coin.Value.Scale))
                .ToHashSet();

            var saveAssetInfosTask = _assetInfosManager.EnsureAdded(assetInfos);

            var fees = evt.TransferCoinsExecutedTransactions
                .Where(t => t.Fees != null)
                .SelectMany
                (
                    t => t.Fees.Select(f => new
                    {
                        TransactionId = t.TransactionId,
                        Fee = f
                    })
                )
                .Concat
                (
                    evt.FailedTransactions
                        .Where(t => t.Fees != null)
                        .SelectMany
                        (
                            t => t.Fees.Select(f => new
                            {
                                TransactionId = t.TransactionId,
                                Fee = f
                            })
                        )
                )
                .Select
                (
                    x => new FeeEnvelope
                    (
                        blockchainType,
                        evt.BlockId,
                        x.TransactionId,
                        x.Fee
                    )
                );

            await Task.WhenAll
            (
                saveTransactionsTask,
                saveCoinsTask,
                saveAssetInfosTask,
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
