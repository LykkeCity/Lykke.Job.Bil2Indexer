using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    public class ChainsCases
    {
        public readonly Dictionary<char, BlockHeader[]>[] Chains;

        public ChainsCases(Dictionary<char, BlockHeader[]>[] chains)
        {
            Chains = chains;
        }

        public BlockHeader[] GetLongestChain(int @case)
        {
            return Chains[@case].Values.OrderByDescending(x => x.Length).First();
        }
    }
}
