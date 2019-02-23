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
        private readonly IBlocksReaderApi _blocksReaderApi;
        private readonly IBlocksRepository _blocksRepository;

        public BlocksProcessor(
            IBlocksReaderApi blocksReaderApi,
            IBlocksRepository blocksRepository)
        {
            _blocksReaderApi = blocksReaderApi;
            _blocksRepository = blocksRepository;
        }

        public async Task ProcessBlockAsync(BlockHeader block)
        {
            var storedBlock = await _blocksRepository.GetOrDefaultAsync(block.Number);

            if (block.Hash == storedBlock?.Hash)
            {
                return;
            }

            var storedPreviousBlock = await _blocksRepository.GetOrDefaultAsync(block.Number - 1);

            if (storedPreviousBlock == null || block.PreviousBlockHash == storedPreviousBlock.Hash)
            {
                await MoveForwardAsync(block);
            }
            else
            {
                await MoveBackwardAsync(block, storedPreviousBlock);
            }
        }

        private async Task MoveForwardAsync(BlockHeader block)
        {
            //var lastValidBlock = await _blocksRepository.GetLastValidOrDefault();

            //if (lastValidBlock?.Number != block.Number)
            //{
            //    // TODO: Defer processing of the block
            //}

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
                    await _blocksRepository.RemoveAsync(storedNextBlock);
                    break;
                }

                currentBlock = storedNextBlock;
            }
            
            await _blocksRepository.SaveAsync(block);
            await _blocksReaderApi.SendAsync(new ReadBlockCommand(nextBlockNumber));
        }

        private async Task MoveBackwardAsync(BlockHeader block, BlockHeader storedPreviousBlock)
        {
            //var lastValidBlock = await _blocksRepository.GetLastValidOrDefault();
            //if (lastValidBlock?.Number != block.Number - 2)
            //{
            //    // TODO: Defer processing of the block
            //}

            await _blocksRepository.RemoveAsync(storedPreviousBlock);

            // TODO: Publish BlockRolledBackEvent

            await _blocksRepository.SaveAsync(block);
            //await _blocksRepository.SetLastValidBlockAsync(block.Number - 2);
            await _blocksReaderApi.SendAsync(new ReadBlockCommand(block.Number - 1));
        }
    }
}
