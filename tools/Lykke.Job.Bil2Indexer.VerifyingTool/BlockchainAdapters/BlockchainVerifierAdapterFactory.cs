using Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters.Bitcoin;
using NBitcoin;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters
{
    public class BlockchainVerifierAdapterFactory
    {
        public BlockchainVerifierAdapterFactory()
        { }

        public IBlockchainVerifierAdapter GetAdapter(string blockchainType, params string[] args)
        {
            switch (blockchainType)
            {
                case "Bitcoin":
                    return new BitcoinBlockchainVerifierAdapter(args[0], Network.GetNetwork(args[1]));
                default:
                    return null;
            }
        }
    }
}
