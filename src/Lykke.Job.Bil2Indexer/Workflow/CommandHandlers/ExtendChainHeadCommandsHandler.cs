﻿using System;
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
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Numerics;
using Lykke.Numerics.Linq;
using FailedTransaction = Lykke.Job.Bil2Indexer.Contract.Events.FailedTransaction;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class ExtendChainHeadCommandsHandler : IMessageHandler<ExtendChainHeadCommand>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IFeeEnvelopesRepository _feeEnvelopesRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly ICoinsRepository _coinsRepository;
        private readonly IReadOnlyDictionary<string, long> _blockNumbersToStartTransactionEventsPublication;
        private readonly IntegrationSettingsProvider _integrationSettingsProvider;
        private readonly ILog _log;

        public ExtendChainHeadCommandsHandler(
            ILogFactory logFactory,
            IChainHeadsRepository chainHeadsRepository,
            ITransactionsRepository transactionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            IBalanceActionsRepository balanceActionsRepository,
            ICoinsRepository coinsRepository,
            IReadOnlyDictionary<string, long> blockNumbersToStartTransactionEventsPublication,
            IntegrationSettingsProvider integrationSettingsProvider)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _transactionsRepository = transactionsRepository;
            _feeEnvelopesRepository = feeEnvelopesRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _coinsRepository = coinsRepository;
            _blockNumbersToStartTransactionEventsPublication = blockNumbersToStartTransactionEventsPublication;
            _integrationSettingsProvider = integrationSettingsProvider;
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
                _log.LogLegacyMessage(command, headers);

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

                if (command.ToBlockNumber >= _blockNumbersToStartTransactionEventsPublication[command.BlockchainType])
                {
                    await PublishTransactionsAsync
                    (
                        command.BlockchainType,
                        command.ToBlockId,
                        command.ToBlockNumber,
                        eventsPublisher
                    );
                }

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
            PaginatedItems<Transaction> transactions = null;
            const int batchSize = 500;
            var transactionsBatch = new List<Transaction>(batchSize);
            var publishBatchTasks = new List<Task>();

            do
            {
                transactions = await _transactionsRepository.GetAllOfBlockAsync
                (
                    blockchainType,
                    blockId,
                    1000,
                    transactions?.Continuation
                );

                foreach (var transaction in transactions.Items)
                {
                    transactionsBatch.Add(transaction);

                    if (transactionsBatch.Count >= batchSize)
                    {
                        // TODO: Add max parallelism constraint

                        var task = PublishTransactionsBatchAsync
                        (
                            blockchainType,
                            blockId,
                            blockNumber,
                            transactionsBatch,
                            publisher
                        );

                        publishBatchTasks.Add(task);

                        transactionsBatch = new List<Transaction>(batchSize);
                    }
                }

            } while (transactions.Continuation != null);

            if (transactionsBatch.Any())
            {
                var task = PublishTransactionsBatchAsync
                (
                    blockchainType,
                    blockId,
                    blockNumber,
                    transactionsBatch,
                    publisher
                );
                publishBatchTasks.Add(task);
            }

            await Task.WhenAll(publishBatchTasks);
        }

        private Task PublishTransactionsBatchAsync(
            string blockchainType, 
            BlockId blockId, 
            long blockNumber, 
            IReadOnlyCollection<Transaction> transactionsBatch,
            IMessagePublisher publisher)
        {
            var integrationSettings = _integrationSettingsProvider.Get(blockchainType);

            switch (integrationSettings.Capabilities.TransferModel)
            {
                case BlockchainTransferModel.Amount:
                    return PublishTransferAmountTransactionsBatchAsync(blockchainType, blockId, blockNumber, transactionsBatch, publisher);
                case BlockchainTransferModel.Coins:
                    return PublishTransferCoinsTransactionsBatchAsync(blockchainType, blockId, blockNumber, transactionsBatch, publisher);
                default:
                    throw new ArgumentOutOfRangeException(nameof(integrationSettings.Capabilities.TransferModel), integrationSettings.Capabilities.TransferModel, "Unknown transfer model");
            }
        }

        private async Task PublishTransferAmountTransactionsBatchAsync(
            string blockchainType,
            string blockId,
            long blockNumber,
            IReadOnlyCollection<Transaction> transactions,
            IMessagePublisher publisher)
        {
            var executedTransactions = new List<TransferAmountExecutedTransaction>(transactions.Count);
            var failedTransactions = new List<Bil2.Contract.BlocksReader.Events.FailedTransaction>(transactions.Count);

            foreach (var transaction in transactions)
            {
                if (transaction.IsTransferAmount)
                {
                    executedTransactions.Add(transaction.AsTransferAmount());
                }
                else if (transaction.IsFailed)
                {
                    failedTransactions.Add(transaction.AsFailed());
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected transaction type: {transaction.Type}");
                }
            }

            var transactionsAccountBalances = await _balanceActionsRepository.GetSomeOfBalancesAsync
            (
                blockchainType,
                executedTransactions.Select(x => x.TransactionId).ToHashSet()
            );

            var executedContractTransactions = executedTransactions
                .Select
                (
                    transaction => GetExecutedTransferAmountTransactionAsync
                    (
                        blockchainType,
                        transactionsAccountBalances,
                        transaction
                    ).ConfigureAwait(false).GetAwaiter().GetResult()
                )
                .ToArray();

            var failedContractTransactions = failedTransactions
                .Select
                (
                    transaction => GetFailedTransactionAsync(blockchainType, transaction).ConfigureAwait(false).GetAwaiter().GetResult()
                )
                .ToArray();

            var evt = new TransactionsBatchEvent
            (
                blockchainType,
                blockId,
                blockNumber,
                executedContractTransactions,
                failedContractTransactions,
                // TODO: Pass block irreversibility flag
                false
            );

            publisher.Publish(evt);
        }

        private async Task PublishTransferCoinsTransactionsBatchAsync(string blockchainType,
            string blockId,
            long blockNumber,
            IReadOnlyCollection<Transaction> transactions,
            IMessagePublisher publisher)
        {
            var executedTransactions = new List<TransferCoinsExecutedTransaction>(transactions.Count);
            var failedTransactions = new List<Bil2.Contract.BlocksReader.Events.FailedTransaction>(transactions.Count);

            foreach (var transaction in transactions)
            {
                if (transaction.IsTransferAmount)
                {
                    executedTransactions.Add(transaction.AsTransferCoins());
                }
                else if (transaction.IsFailed)
                {
                    failedTransactions.Add(transaction.AsFailed());
                }
                else
                {
                    throw new InvalidOperationException($"Unexpected transaction type: {transaction.Type}");
                }
            }

            var (transactionsAccountBalances, coinsSpentByTransactions) = await TaskExecution.WhenAll
            (
                _balanceActionsRepository.GetSomeOfBalancesAsync
                (
                    blockchainType,
                    executedTransactions.Select(x => x.TransactionId).ToHashSet()
                ),
                _coinsRepository.GetSomeOfAsync
                (
                    blockchainType,
                    executedTransactions.SelectMany(x => x.SpentCoins).ToArray()
                )
            );

            var transactionsSpentCoins = coinsSpentByTransactions
                .GroupBy(x => x.Id.TransactionId)
                .ToDictionary
                (
                    g => g.Key,
                    g => g.Select(x => x)
                );

            var executedContractTransactions = executedTransactions
                .Select
                (
                    transaction => GetExecutedTransferCoinsTransactionAsync
                    (
                        blockchainType,
                        transactionsAccountBalances,
                        transaction,
                        transactionsSpentCoins
                    ).ConfigureAwait(false).GetAwaiter().GetResult()
                )
                .ToArray();

            var failedContractTransactions = failedTransactions
                .Select
                (
                    transaction => GetFailedTransactionAsync(blockchainType, transaction).ConfigureAwait(false).GetAwaiter().GetResult()
                )
                .ToArray();

            var evt = new TransactionsBatchEvent
            (
                blockchainType,
                blockId,
                blockNumber,
                executedContractTransactions,
                failedContractTransactions,
                // TODO: Pass block irreversibility flag
                false
            );

            publisher.Publish(evt);
        }

        private async Task<ExecutedTransaction> GetExecutedTransferAmountTransactionAsync(
            string blockchainType, 
            IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>> transactionsAccountBalances,
            TransferAmountExecutedTransaction transaction)
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

            return new ExecutedTransaction
            (
                transaction.TransactionNumber,
                transaction.TransactionId,
                balanceUpdates.ToArray(),
                fees.Select(x => x.Fee).ToArray()
            );
        }

        private async Task<ExecutedTransaction> GetExecutedTransferCoinsTransactionAsync(
            string blockchainType, 
            IReadOnlyDictionary<TransactionId, IReadOnlyDictionary<AccountId, Money>> transactionsAccountBalances,
            TransferCoinsExecutedTransaction transaction, 
            IReadOnlyDictionary<TransactionId, IEnumerable<Coin>> transactionsSpentCoins)
        {
            if (!transactionsAccountBalances.TryGetValue(transaction.TransactionId, out var transactionAccountBalances))
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
            
            return new ExecutedTransaction
            (
                transaction.TransactionNumber,
                transaction.TransactionId,
                balanceUpdates.ToArray(),
                fees.Select(x => x.Fee).ToArray()
            );
        }

        private async Task<FailedTransaction> GetFailedTransactionAsync(
            string blockchainType,
            Bil2.Contract.BlocksReader.Events.FailedTransaction transaction)
        {
            var fees = await _feeEnvelopesRepository.GetTransactionFeesAsync(blockchainType, transaction.TransactionId);
            return new FailedTransaction
            (
                transaction.TransactionNumber,
                transaction.TransactionId,
                transaction.ErrorCode,
                transaction.ErrorMessage,
                fees.Select(x => x.Fee).ToArray()
            );
        }
    }
}
