using System;
using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    public class BlocksProcessor : IBlocksProcessor
    {
        private readonly int _startBlock;
        private readonly IBlocksReaderApi _blocksReaderApi;
        private readonly IBlocksRepository _blocksRepository;
        private readonly IBlocksDeduplicationRepository _blocksDeduplicationRepository;

        public BlocksProcessor(
            int startBlock,
            IBlocksReaderApi blocksReaderApi,
            IBlocksRepository blocksRepository,
            IBlocksDeduplicationRepository blocksDeduplicationRepository)
        {
            _startBlock = startBlock;
            _blocksReaderApi = blocksReaderApi;
            _blocksRepository = blocksRepository;
            _blocksDeduplicationRepository = blocksDeduplicationRepository;
        }

        public async Task ProcessBlockAsync(BlockHeader block)
        {
            if (await _blocksDeduplicationRepository.IsProcessedAsync(block.Hash))
            {
                return;
            }

            var (storedPreviousBlock, storedHeadBlock) = await TaskExecution.WhenAll
            (
                _blocksRepository.GetOrDefaultAsync(block.Number - 1),
                _blocksRepository.GetHeadOrDefaultAsync()
            );

            if (storedHeadBlock?.Hash != storedPreviousBlock?.Hash)
            {
                throw new InvalidOperationException($"prev: {storedPreviousBlock?.Hash}, head: {storedHeadBlock?.Hash}");
            }

            if (storedPreviousBlock == null || block.PreviousBlockHash == storedPreviousBlock.Hash)
            {
                await MoveForwardAsync(storedHeadBlock, block);
            }
            else
            {
                await MoveBackwardAsync(storedHeadBlock, block, storedPreviousBlock);
            }

            await _blocksDeduplicationRepository.MarkAsProcessedAsync(block.Hash);
        }

        private async Task MoveForwardAsync(BlockHeader storedHeadBlock, BlockHeader block)
        {
            if (storedHeadBlock == null && block.Number != _startBlock || 
                storedHeadBlock != null && block.Number != storedHeadBlock.Number + 1)
            {
                throw new InvalidOperationException($"Disordered block on forward turn: {block.Number}, waiting for block: {storedHeadBlock?.Number + 1 ?? _startBlock}");
            }

            long nextBlockNumber;
            var currentBlock = block;

            // Is next block already read? Skipping it if so.
            while (true)
            {
                nextBlockNumber = currentBlock.Number + 1;
                var storedNextBlock = await _blocksRepository.GetOrDefaultAsync(nextBlockNumber);

                if (storedNextBlock == null)
                {
                    break;
                }

                // Is next block already stored, but belongs to another chain? Removing it if so.
                if (storedNextBlock.PreviousBlockHash != currentBlock.Hash)
                {
                    await Task.WhenAll
                    (
                        _blocksRepository.RemoveAsync(storedNextBlock),
                        _blocksDeduplicationRepository.MarkAsNotProcessedAsync(storedNextBlock.Hash)
                    );
                    break;
                }

                currentBlock = storedNextBlock;
            }

            await Task.WhenAll
            (
                _blocksRepository.SaveAsync(block),
                _blocksRepository.SetHeadAsync(currentBlock, storedHeadBlock)
            );

            // Should be last step
            await _blocksReaderApi.SendAsync(new ReadBlockCommand(nextBlockNumber));
        }

        private async Task MoveBackwardAsync(BlockHeader storedHeadBlock, BlockHeader block, BlockHeader storedPreviousBlock)
        {
            if (block.Number != storedHeadBlock.Number + 1)
            {
                throw new InvalidOperationException($"Disordered block on backward turn: {block.Number}, waiting for block: {storedHeadBlock.Number + 1}");
            }

            var getNewHeadBlockTask = _blocksRepository.GetOrDefaultAsync(block.Number - 2);
            var removePreviousBlockTask = _blocksRepository.RemoveAsync(storedPreviousBlock);
            var markPreviousBlockAsNotProcessedTask = _blocksDeduplicationRepository.MarkAsNotProcessedAsync(storedPreviousBlock.Hash);
            var saveBlockTask = _blocksRepository.SaveAsync(block);

            var newHeadBlock = await getNewHeadBlockTask;

            await Task.WhenAll
            (
                removePreviousBlockTask,
                markPreviousBlockAsNotProcessedTask,
                saveBlockTask,
                _blocksRepository.SetHeadAsync(newHeadBlock, storedHeadBlock)
            );

            // Should be last step
            await _blocksReaderApi.SendAsync(new ReadBlockCommand(block.Number - 1));

            // TODO: Publish BlockRolledBackEvent
        }
    }
}
