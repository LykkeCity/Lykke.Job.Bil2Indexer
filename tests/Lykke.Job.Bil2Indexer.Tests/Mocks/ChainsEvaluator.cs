using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    internal class ChainsEvaluator
    {
        public Func<char, long, bool> ForceSwitchChain { get; set; }
        public Func<InMemoryBlocksQueue, Dictionary<char, BlockHeader[]>, char, BlockHeader, bool> CustomBlockProcessing { get; set; }
        public int Case { get; set; }

        private readonly Dictionary<char, BlockHeader[]>[] _chains;
        private readonly InMemoryBlocksQueue _blocksQueue;

        private char _activeChain;

        public ChainsEvaluator(
            Dictionary<char, BlockHeader[]>[] chains,
            InMemoryBlocksQueue blocksQueue)
        {
            _chains = chains;
            _blocksQueue = blocksQueue;
            
            _activeChain = 'A';
        }

        public void EvaluateBlock(long blockNumber)
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

                var customBlockProcessingResult = CustomBlockProcessing?.Invoke(_blocksQueue, _chains[Case], _activeChain, block) ?? true;

                if (customBlockProcessingResult)
                {
                    _blocksQueue.Publish(block);
                }
            }
            else
            {
                Console.WriteLine("Chain is finished");

                _blocksQueue.Stop();
            }
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
