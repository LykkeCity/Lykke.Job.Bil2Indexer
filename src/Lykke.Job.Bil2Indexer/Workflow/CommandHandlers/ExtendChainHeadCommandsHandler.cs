using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
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

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class ExtendChainHeadCommandsHandler : IMessageHandler<ExtendChainHeadCommand>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IFeeEnvelopesRepository _feeEnvelopesRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly ICoinsRepository _coinsRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;

        public ExtendChainHeadCommandsHandler(
            IChainHeadsRepository chainHeadsRepository,
            ITransactionsRepository transactionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            IBalanceActionsRepository balanceActionsRepository,
            ICoinsRepository coinsRepository,
            IntegrationSettingsProvider settingsProvider)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _transactionsRepository = transactionsRepository;
            _feeEnvelopesRepository = feeEnvelopesRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _coinsRepository = coinsRepository;
            _settingsProvider = settingsProvider;
        }

        public async Task HandleAsync(ExtendChainHeadCommand command, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var chainHead = await _chainHeadsRepository.GetAsync(command.BlockchainType);

            if (!(chainHead.CanExtendTo(command.ToBlockNumber) ||
                chainHead.IsOnBlock(command.ToBlockNumber)))
            {
                // TODO: Not sure yet what to do here. Probably we need to check block header state.
                // We need to determine somehow if this message is outdated or premature and ignore or 
                // retry it correspondingly.
                return;
            }

            if (chainHead.CanExtendTo(command.ToBlockNumber))
            {
                await Task.WhenAll
                (
                    PublishExecutedTransactionsAsync(command.BlockchainType, command.ToBlockId, command.ToBlockNumber, replyPublisher),
                    PublishFailedTransactionsAsync(command.BlockchainType, command.ToBlockId, command.ToBlockNumber, replyPublisher)
                );
                
                chainHead.ExtendTo(command.ToBlockNumber, command.ToBlockId);

                // TODO: Update balance snapshots

                replyPublisher.Publish(new ChainHeadExtendedEvent
                {
                    BlockchainType = command.BlockchainType,
                    ChainHeadSequence = chainHead.Version,
                    BlockNumber = command.ToBlockNumber,
                    BlockId = command.ToBlockId,
                    PreviousBlockId = chainHead.PreviousBlockId
                });

                await _chainHeadsRepository.SaveAsync(chainHead);
            }
        }

        private Task PublishExecutedTransactionsAsync(string blockchainType, string blockId, long blockNumber, IMessagePublisher publisher)
        {
            var settings = _settingsProvider.Get(blockchainType);

            if (settings.Capabilities.TransferModel == BlockchainTransferModel.Amount)
            {
                return PublishTransferAmountTransactionsAsync(blockchainType, blockId, blockNumber, publisher);
            }

            if (settings.Capabilities.TransferModel == BlockchainTransferModel.Coins)
            {
                return PublishTransferCoinsTransactionsAsync(blockchainType, blockId, blockNumber, publisher);
            }

            throw new ArgumentOutOfRangeException(nameof(settings.Capabilities.TransferModel), settings.Capabilities.TransferModel, "");
        }

        private async Task PublishTransferCoinsTransactionsAsync(string blockchainType, string blockId, long blockNumber, IMessagePublisher publisher)
        {
            PaginatedItems<TransferCoinsTransactionExecutedEvent> transactions = null;

            do
            {
                transactions = await _transactionsRepository.GetTransferCoinsTransactionsOfBlockAsync
                (
                    blockchainType,
                    blockId,
                    500,
                    transactions?.Continuation
                );

                var (transactionsAccountBalances, coinsSpentByTransactions) = await TaskExecution.WhenAll
                (
                    _balanceActionsRepository.GetSomeOfBalancesAsync
                    (
                        blockchainType,
                        transactions.Items.Select(x => x.TransactionId).ToHashSet()
                    ),
                    _coinsRepository.GetSomeOfAsync
                    (
                        blockchainType,
                        transactions.Items.SelectMany(x => x.SpentCoins).ToArray()
                    )
                );

                var transactionsSpentCoins = coinsSpentByTransactions
                    .GroupBy(x => x.Id.TransactionId)
                    .ToDictionary
                    (
                        g => g.Key,
                        g => g.Select(x => x)
                    );

                // TODO: Make in parallel

                foreach (var transaction in transactions.Items)
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
                                        address: x.Address,
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
                                        address: x.Address,
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

            } while (transactions.Continuation != null);
        }

        private async Task PublishTransferAmountTransactionsAsync(string blockchainType, string blockId, long blockNumber, IMessagePublisher publisher)
        {
            PaginatedItems<TransferAmountTransactionExecutedEvent> transactions = null;

            do
            {
                transactions = await _transactionsRepository.GetTransferAmountTransactionsOfBlockAsync
                (
                    blockchainType,
                    blockId,
                    500,
                    transactions?.Continuation
                );

                var transactionsAccountBalances = await _balanceActionsRepository.GetSomeOfBalancesAsync
                (
                    blockchainType,
                    transactions.Items.Select(x => x.TransactionId).ToHashSet()
                );

                // TODO: Make in parallel

                foreach (var transaction in transactions.Items)
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
                                        x.Address,
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

            } while (transactions.Continuation != null);
        }

        private async Task PublishFailedTransactionsAsync(string blockchainType, string blockId, long blockNumber, IMessagePublisher publisher)
        {
            PaginatedItems<Bil2.Contract.BlocksReader.Events.TransactionFailedEvent> transactions = null;

            do
            {
                transactions = await _transactionsRepository.GetFailedTransactionsOfBlockAsync
                (
                    blockchainType,
                    blockId,
                    500,
                    transactions?.Continuation
                );

                // TODO: Make in parallel

                foreach (var transaction in transactions.Items)
                {
                    // TODO: Get batch of transactions

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
               
            } while (transactions.Continuation != null);
        }
    }
}
