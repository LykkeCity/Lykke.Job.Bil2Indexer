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
        public Func<IChainCrawler, Dictionary<char, BlockHeader[]>, char, BlockHeader, Task<bool>> CustomBlockProcessing { get; set; }
        public int Case { get; set; }

        private readonly Dictionary<char, BlockHeader[]>[] _chains;
        private readonly IChainCrawler _chainCrawler;

        private char _activeChain;

        public ChainsEvaluator(
            Dictionary<char, BlockHeader[]>[] chains,
            IChainCrawler chainCrawler)
        {
            _chains = chains;
            _chainCrawler = chainCrawler;


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

                var customBlockProcessingTask = CustomBlockProcessing?.Invoke(_chainCrawler, _chains[Case], _activeChain, block) ??
                                                Task.FromResult(true);

                if (await customBlockProcessingTask)
                {
                    await _chainCrawler.ProcessBlockAsync(block);
                }

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
