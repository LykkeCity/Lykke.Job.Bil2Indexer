﻿using System;
using System.Threading.Tasks;
using Lykke.Bil2.RabbitMq.Publication;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;
using Lykke.Job.Bil2Indexer.Infrastructure;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Lykke.Job.Bil2Indexer.Workflow.Events;

namespace Lykke.Job.Bil2Indexer.Workflow.EventHandlers
{
    public class BlockAssembledEventsHandler : IMessageHandler<BlockAssembledEvent>
    {
        private readonly ICrawlersManager _crawlersManager;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly IntegrationSettingsProvider _settingsProvider;
        private readonly IChainHeadsRepository _chainHeadsRepository;

        public BlockAssembledEventsHandler(
            ICrawlersManager crawlersManager,
            IBlockHeadersRepository blockHeadersRepository,
            IntegrationSettingsProvider settingsProvider,
            IChainHeadsRepository chainHeadsRepository)
        {
            _crawlersManager = crawlersManager;
            _blockHeadersRepository = blockHeadersRepository;
            _settingsProvider = settingsProvider;
            _chainHeadsRepository = chainHeadsRepository;
        }

        public async Task HandleAsync(BlockAssembledEvent evt, MessageHeaders headers, IMessagePublisher replyPublisher)
        {
            var messageCorrelationId = CrawlerCorrelationId.Parse(headers.CorrelationId);

            var newBlock = await _blockHeadersRepository.GetAsync(evt.BlockchainType, evt.BlockId);
            var (previousBlock, crawler) = await TaskExecution.WhenAll
            (
                _blockHeadersRepository.GetOrDefaultAsync(evt.BlockchainType, newBlock.Number - 1),
                _crawlersManager.GetCrawlerAsync(evt.BlockchainType, newBlock.Number)
            );

            if (!crawler.GetCorrelationId().Equals(messageCorrelationId))
            {
                // Disordered message, we should ignore it.
                return;
            }

            long nextBlockNumber;
            var crawlingDirection = crawler.EvaluateDirection(previousBlock, newBlock);

            switch (crawlingDirection)
            {
                case CrawlingDirection.Forward:
                    nextBlockNumber = await crawler.EvaluateNextBlockToMoveForwardAsync
                    (
                        newBlock,
                        blockNumber => _blockHeadersRepository.GetOrDefaultAsync(evt.BlockchainType, blockNumber),
                        blockToRollback => replyPublisher.Publish
                        (
                            new RollbackBlockCommand
                            {
                                BlockchainType = evt.BlockchainType,
                                BlockId = blockToRollback.Id,
                                BlockNumber = blockToRollback.Number,
                                PreviousBlockId = blockToRollback.PreviousBlockId
                            }
                        )
                    );
                    break;

                case CrawlingDirection.Backward:
                    nextBlockNumber = crawler.EvaluateNextBlockToMoveBackward
                    (
                        newBlock,
                        previousBlock,
                        blockToRollback => replyPublisher.Publish
                        (
                            new RollbackBlockCommand
                            {
                                BlockchainType = evt.BlockchainType,
                                BlockId = blockToRollback.Id,
                                BlockNumber = blockToRollback.Number,
                                PreviousBlockId = blockToRollback.PreviousBlockId
                            }
                        )
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(crawlingDirection), crawlingDirection, "Unknown value");
            }
            
            replyPublisher.Publish(new MoveCrawlerCommand
            {
                BlockchainType = evt.BlockchainType,
                NextBlockNumber = nextBlockNumber
            });

            // TODO: Should be published only on forward movement?

            var settings = _settingsProvider.Get(evt.BlockchainType);

            if (settings.Capabilities.TransferModel == BlockchainTransferModel.Coins)
            {
                replyPublisher.Publish(new ExecuteTransferCoinsBlockCommand
                {
                    BlockchainType = evt.BlockchainType,
                    BlockId = newBlock.Id,
                    BlockVersion = newBlock.Version
                });
            }
            else if(settings.Capabilities.TransferModel == BlockchainTransferModel.Amount)
            {
                var chainHead = await _chainHeadsRepository.GetAsync(evt.BlockchainType);

                if (chainHead.CanExtendTo(newBlock.Number))
                {
                    replyPublisher.Publish(new ExtendChainHeadCommand
                    {
                        BlockchainType = evt.BlockchainType,
                        ToBlockNumber = newBlock.Number,
                        ToBlockId = newBlock.Id,
                        ChainHeadVersion = chainHead.Version
                    });
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(settings.Capabilities.TransferModel), settings.Capabilities.TransferModel, "Unknown transfer model");
            }
        }
    }
}
