using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Bil2.SharedDomain;
using Lykke.Common.Log;
using Lykke.Job.Bil2Indexer.Contract;
using Lykke.Job.Bil2Indexer.Contract.Events;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Infrastructure;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Numerics;
using Lykke.Numerics.Linq;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class ExtendChainHeadCommandsHandler : IMessageHandler<ExtendChainHeadCommand>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IFeeEnvelopesRepository _feeEnvelopesRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly ICoinsRepository _coinsRepository;
        private readonly ILog _log;

        public ExtendChainHeadCommandsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository,
            ITransactionsRepository transactionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            IBalanceActionsRepository balanceActionsRepository,
            ICoinsRepository coinsRepository)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _transactionsRepository = transactionsRepository;
            _feeEnvelopesRepository = feeEnvelopesRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _coinsRepository = coinsRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task<MessageHandlingResult> HandleAsync(ExtendChainHeadCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = ChainHeadCorrelationId.Parse(headers.CorrelationId);
            var chainHead = await _chainHeadsRepository.GetAsync(command.BlockchainType);
            var chainHeadCorrelationId = chainHead.GetCorrelationId();

            if (messageCorrelationId.IsLegacyRelativeTo(chainHeadCorrelationId) &&
                // In case of retry after chain head sequence incremented and saved,
                // the message is became previous relative to the updated chain head,
                // we should process the message, since we not sure if the events
                // are published.
                !messageCorrelationId.IsPreviousOf(chainHeadCorrelationId))
            {
                // The message is legacy, it already was processed for sure, we can ignore it.
                return MessageHandlingResult.Success();
            }

            if(messageCorrelationId.IsPrematureRelativeTo(chainHeadCorrelationId))
            {
                // The message is premature, it can't be processed yet, we should retry it later.
                return MessageHandlingResult.TransientFailure();
            }

            if (messageCorrelationId.IsTheSameAs(chainHeadCorrelationId))
            {
                chainHead.ExtendTo(command.ToBlockNumber, command.ToBlockId);

                // TODO: Update balance snapshots

                await _chainHeadsRepository.SaveAsync(chainHead);

                chainHeadCorrelationId = chainHead.GetCorrelationId();
            }

            if (messageCorrelationId.IsPreviousOf(chainHeadCorrelationId))
            {
                _log.Info("Chain head extended", chainHead);

                var eventsPublisher = replyPublisher.ChangeCorrelationId(chainHeadCorrelationId.ToString());

                await PublishTransactionsAsync
                (
                    command.BlockchainType,
                    command.ToBlockId,
                    command.ToBlockNumber,
                    eventsPublisher
                );

                eventsPublisher.Publish(new ChainHeadExtendedEvent
                {
                    BlockchainType = command.BlockchainType,
                    ChainHeadSequence = chainHead.Version,
                    BlockNumber = command.ToBlockNumber,
                    BlockId = command.ToBlockId,
                    PreviousBlockId = chainHead.PreviousBlockId
                });
            }
            
            return MessageHandlingResult.Success();
        }

        private async Task PublishTransactionsAsync(string blockchainType,
            string blockId, 
            long blockNumber,
            IMessagePublisher publisher)
        {
            PaginatedItems<TransactionEnvelope> envelopes = null;

            do
            {
                envelopes = await _transactionsRepository.GetAllOfBlockAsync
                (
                    blockchainType,
                    blockId,
                    500,
                    envelopes?.Continuation
                );

                var transferAmountTransactions = new ConcurrentBag<TransferAmountTransactionExecutedEvent>();
                var transferCoinsTransactions = new ConcurrentBag<TransferCoinsTransactionExecutedEvent>();
                var failedTransactions = new ConcurrentBag<Bil2.Contract.BlocksReader.Events.TransactionFailedEvent>();

                await envelopes.Items.ForEachAsync(
                    8,
                    envelope =>
                    {
                        if (envelope.IsTransferAmount)
                        {
                            transferAmountTransactions.Add(envelope.AsTransferAmount());
                        } 
                        else if (envelope.IsTransferCoins)
                        {
                            transferCoinsTransactions.Add(envelope.AsTransferCoins());
                        }
                        else if (envelope.IsFailed)
                        {
                            failedTransactions.Add(envelope.AsFailed());
                        }
                        else
                        {
                            throw new InvalidOperationException("Unknown transaction type");
                        }

                        return Task.CompletedTask;
                    });

                await Task.WhenAll
                (
                    PublishTransferAmountTransactionsAsync(blockchainType, blockId, blockNumber, transferAmountTransactions, publisher),
                    PublishTransferCoinsTransactionsAsync(blockchainType, blockId, blockNumber, transferCoinsTransactions, publisher),
                    PublishFailedTransactionsAsync(blockchainType, blockId, blockNumber, failedTransactions, publisher)
                );

            } while (envelopes.Continuation != null);
        }

        private async Task PublishTransferAmountTransactionsAsync(
            string blockchainType,
            string blockId,
            long blockNumber,
            IReadOnlyCollection<TransferAmountTransactionExecutedEvent> transactions,
            IMessagePublisher publisher)
        {
            var transactionsAccountBalances = await _balanceActionsRepository.GetSomeOfBalancesAsync
            (
                blockchainType,
                transactions.Select(x => x.TransactionId).ToHashSet()
            );

            await transactions.ForEachAsync
            (
                4,
                transaction => PublishTransferAmountTransactionAsync
                (
                    blockchainType,
                    blockId,
                    blockNumber,
                    publisher,
                    transactionsAccountBalances,
                    transaction
                )
            );
        }

        private async Task PublishTransferAmountTransactionAsync(
            string blockchainType, 
            string blockId, 
            long blockNumber,
            IMessagePublisher publisher, 
            IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>> transactionsAccountBalances,
            TransferAmountTransactionExecutedEvent transaction)
        {
            if (!transactionsAccountBalances.TryGetValue(transaction.TransactionId, out var transactionAccountBalances))
            {
                transactionAccountBalances = new Dictionary<AccountId, Money>();
            }

            var transactionAccountTransfers = transaction.BalanceChanges
                .Where(x => x.Address != null)
                .GroupBy(x => new AccountId(x.Address, x.Asset))
                .ToDictionary
                (
                    g => g.Key,
                    g => g
                        .Select
                        (
                            x => new Transfer
                            (
                                x.TransferId,
                                x.Value,
                                x.Tag,
                                x.TagType,
                                x.Nonce
                            )
                        )
                        .ToArray()
                );

            var balanceUpdates = transactionAccountBalances
                .Select(accountBalance =>
                {
                    var accountId = accountBalance.Key;
                    var balance = accountBalance.Value;
                    var transfers = transactionAccountTransfers[accountId];
                    var oldBalance = balance - transfers.Sum(x => x.Value);

                    return new BalanceUpdate
                    (
                        accountId: accountId,
                        oldBalance: oldBalance,
                        newBalance: balance,
                        transfers: transfers,
                        spentCoins: null,
                        receivedCoins: null
                    );
                });

            // TODO: Get batch of transactions

            var fees = await _feeEnvelopesRepository.GetTransactionFeesAsync(blockchainType, transaction.TransactionId);
            var evt = new TransactionExecutedEvent
            (
                blockchainType,
                blockId,
                blockNumber,
                transaction.TransactionNumber,
                transaction.TransactionId,
                balanceUpdates.ToArray(),
                fees.Select(x => x.Fee).ToArray()
            );

            publisher.Publish(evt);
        }

        private async Task PublishTransferCoinsTransactionsAsync(string blockchainType,
            string blockId,
            long blockNumber,
            IReadOnlyCollection<TransferCoinsTransactionExecutedEvent> transactions,
            IMessagePublisher publisher)
        {
            var (transactionsAccountBalances, coinsSpentByTransactions) = await TaskExecution.WhenAll
            (
                _balanceActionsRepository.GetSomeOfBalancesAsync
                (
                    blockchainType,
                    transactions.Select(x => x.TransactionId).ToHashSet()
                ),
                _coinsRepository.GetSomeOfAsync
                (
                    blockchainType,
                    transactions.SelectMany(x => x.SpentCoins).ToArray()
                )
            );

            var transactionsSpentCoins = coinsSpentByTransactions
                .GroupBy(x => x.Id.TransactionId)
                .ToDictionary
                (
                    g => g.Key,
                    g => g.Select(x => x)
                );

            await transactions.ForEachAsync
            (
                4,
                transaction => PublishTransferCoinsTransactionAsync
                (
                    blockchainType,
                    blockId,
                    blockNumber,
                    publisher,
                    transactionsAccountBalances,
                    transaction,
                    transactionsSpentCoins
                )
            );
        }

        private async Task PublishTransferCoinsTransactionAsync(
            string blockchainType, 
            string blockId,
            long blockNumber,
            IMessagePublisher publisher, 
            IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>> transactionsAccountBalances,
            TransferCoinsTransactionExecutedEvent transaction, 
            Dictionary<TransactionId, IEnumerable<Coin>> transactionsSpentCoins)
        {
            if (!transactionsAccountBalances.TryGetValue(transaction.TransactionId,
                out var transactionAccountBalances))
            {
                transactionAccountBalances = new Dictionary<AccountId, Money>();
            }

            if (!transactionsSpentCoins.TryGetValue(transaction.TransactionId, out var transactionSpentCoins))
            {
                transactionSpentCoins = Enumerable.Empty<Coin>();
            }

            var transactionAccountSpentCoins = transactionSpentCoins
                .Where(x => x.Address != null)
                .GroupBy(x => new AccountId(x.Address, x.Asset))
                .ToDictionary
                (
                    g => g.Key,
                    g => g
                        .Select
                        (
                            x => new SpentCoin
                            (
                                id: x.Id,
                                value: x.Value,
                                tag: x.AddressTag,
                                tagType: x.AddressTagType,
                                nonce: x.AddressNonce
                            )
                        )
                        .ToArray()
                );

            var transactionAccountReceivedCoins = transaction.ReceivedCoins
                .Where(x => x.Address != null)
                .GroupBy(x => new AccountId(x.Address, x.Asset))
                .ToDictionary
                (
                    g => g.Key,
                    g => g
                        .Select
                        (
                            x => new Contract.ReceivedCoin
                            (
                                number: x.CoinNumber,
                                value: x.Value,
                                tag: x.AddressTag,
                                tagType: x.AddressTagType,
                                nonce: x.AddressNonce
                            )
                        )
                        .ToArray()
                );

            var balanceUpdates = transactionAccountBalances
                .Select(accountBalance =>
                {
                    var accountId = accountBalance.Key;
                    var balance = accountBalance.Value;

                    if (!transactionAccountSpentCoins.TryGetValue(accountId, out var spentCoins))
                    {
                        spentCoins = Array.Empty<SpentCoin>();
                    }

                    if (!transactionAccountReceivedCoins.TryGetValue(accountId, out var receivedCoins))
                    {
                        receivedCoins = Array.Empty<Contract.ReceivedCoin>();
                    }

                    var spentAmount = spentCoins.Sum(x => x.Value);
                    var receivedAmount = receivedCoins.Sum(x => x.Value);
                    var oldBalance = balance + spentAmount - receivedAmount;

                    return new BalanceUpdate
                    (
                        accountId: accountId,
                        oldBalance: oldBalance,
                        newBalance: balance,
                        transfers: null,
                        spentCoins: spentCoins,
                        receivedCoins: receivedCoins
                    );
                });

            // TODO: Get batch of transactions

            var fees = await _feeEnvelopesRepository.GetTransactionFeesAsync(blockchainType, transaction.TransactionId);
            var evt = new TransactionExecutedEvent
            (
                blockchainType,
                blockId,
                blockNumber,
                transaction.TransactionNumber,
                transaction.TransactionId,
                balanceUpdates.ToArray(),
                fees.Select(x => x.Fee).ToArray()
            );

            publisher.Publish(evt);
        }

        private Task PublishFailedTransactionsAsync(string blockchainType,
            string blockId,
            long blockNumber,
            IEnumerable<Bil2.Contract.BlocksReader.Events.TransactionFailedEvent> transactions,
            IMessagePublisher publisher)
        {
            return transactions.ForEachAsync
            (
                4,
                async transaction =>
                {
                    var fees = await _feeEnvelopesRepository.GetTransactionFeesAsync(blockchainType, transaction.TransactionId);
                    var evt = new Contract.Events.TransactionFailedEvent
                    (
                        blockchainType,
                        blockId,
                        blockNumber,
                        transaction.TransactionNumber,
                        transaction.TransactionId,
                        transaction.ErrorCode,
                        transaction.ErrorMessage,
                        fees.Select(x => x.Fee).ToArray()
                    );

                    publisher.Publish(evt);
                }
            );
        }
    }
}
