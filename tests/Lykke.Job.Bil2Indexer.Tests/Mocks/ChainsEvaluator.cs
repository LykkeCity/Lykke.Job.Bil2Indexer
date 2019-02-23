using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Services;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    internal class ChainsEvaluator
    {
        public Func<char, long, bool> ForceSwitchChain { get; set; }
        public int Case { get; set; }

        private readonly Dictionary<char, BlockHeader[]>[] _chains;
        private readonly IBlocksProcessor _blocksProcessor;

        private char _activeChain;

        public ChainsEvaluator(
            Dictionary<char, BlockHeader[]>[] chains,
            IBlocksProcessor blocksProcessor)
        {
            _chains = chains;
            _blocksProcessor = blocksProcessor;


            _activeChain = 'A';
        }

        public async Task<bool> EvaluateBlockAsync(long blockNumber)
        {
            if (ForceSwitchChain?.Invoke(_activeChain, blockNumber) == true)
            {
                _activeChain++;
            }

            var block = GetBlockOrDefault(Case, _activeChain, blockNumber) ??
                        GetBlockOrDefault(Case, ++_activeChain, blockNumber);

            if (block != null)
            {
                Console.WriteLine($"Processing: {block}");

                await _blocksProcessor.ProcessBlockAsync(block);
                return true;
            }

            Console.WriteLine("Chain is finished");
            return false;
        }

        private BlockHeader GetBlockOrDefault(int @case, char chain, long blockNumber)
        {
            if (_chains[@case].TryGetValue(chain, out var blocks))
            {
                return blocks.FirstOrDefault(b => b.Number == blockNumber);
            }

            return null;
        }
    }
}
