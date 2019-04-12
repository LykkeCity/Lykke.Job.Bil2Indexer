using System;
using System.Numerics;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.VerifyingTool
{
    class Program
    {
        static void Main(string[] args)
        {
            BigInteger fromBlock = BigInteger.Parse(args[0]);
            BigInteger toBlock = BigInteger.Parse(args[1]);
            string blockchainType = args[2];

            IBalanceActionsRepository balanceActionsRepository;
            IBlockHeadersRepository blockHeadersRepository;
            ICoinsRepository coinsRepository;
            ICrawlersRepository crawlersRepository;
            ITransactionsRepository transactionsRepository;
            IChainHeadsRepository chainHeadsRepository;
        }
    }
}
