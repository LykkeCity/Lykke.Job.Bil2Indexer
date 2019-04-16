using System;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.CommandHandlers
{
    public class ExtendChainHeadCommandsHandler : IMessageHandler<ExtendChainHeadCommand>
    {
        private readonly IChainHeadsRepository _chainHeadsRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IFeeEnvelopesRepository _feeEnvelopesRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;

        public ExtendChainHeadCommandsHandler(
            IChainHeadsRepository chainHeadsRepository,
            ITransactionsRepository transactionsRepository,
            IFeeEnvelopesRepository feeEnvelopesRepository,
            IBalanceActionsRepository balanceActionsRepository,
            IntegrationSettingsProvider settingsProvider)
        {
            _chainHeadsRepository = chainHeadsRepository;
            _transactionsRepository = transactionsRepository;
            _feeEnvelopesRepository = feeEnvelopesRepository;
            _balanceActionsRepository = balanceActionsRepository;
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

                await _chainHeadsRepository.SaveAsync(chainHead);
            }

            // TODO: Update balance snapshots

            replyPublisher.Publish(new ChainHeadExtendedEvent
            {
                BlockchainType = command.BlockchainType,
                ChainHeadSequence = chainHead.Version,
                ToBlockNumber = command.ToBlockNumber,
                ToBlockId = command.ToBlockId
            });
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
            //PaginatedItems<TransferCoinsTransactionExecutedEvent> transactions = null;

            //do
            //{
            //    transactions = await _transactionsRepository.GetTransferCoinsTransactionsOfBlockAsync
            //    (
            //        blockchainType,
            //        blockId,
            //        500,
            //        transactions?.Continuation
            //    );

            //    // TODO: Make in parallel

            //    var accounts = await _balanceActionsRepository.GetBalancesAsync
            //    (
            //        blockchainType,

            //    );

            //    foreach (var transaction in transactions.Items)
            //    {
            //        // TODO: Get batch of transactions

            //        var fees = await _feeEnvelopesRepository.GetTransactionFeesAsync(blockchainType, transaction.TransactionId);
            //        var evt = new Contract.Events.TransactionExecutedEvent
            //        (
            //            blockchainType,
            //            blockId,
            //            blockNumber,
            //            transaction.TransactionNumber,
            //            transaction.TransactionId,
            //            transaction.ErrorCode,
            //            transaction.ErrorMessage,
            //            fees.Select(x => x.Fee).ToArray()
            //        );

            //        publisher.Publish(evt);
            //    }
               
            //} while (transactions.Continuation != null);
        }

        private Task PublishTransferAmountTransactionsAsync(string blockchainType, string blockId, long blockNumber, IMessagePublisher publisher)
        {
            throw new NotImplementedException();
        }

        private async Task PublishFailedTransactionsAsync(string blockchainType, string blockId, long blockNumber, IMessagePublisher publisher)
        {
            PaginatedItems<TransactionFailedEvent> transactions = null;

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
