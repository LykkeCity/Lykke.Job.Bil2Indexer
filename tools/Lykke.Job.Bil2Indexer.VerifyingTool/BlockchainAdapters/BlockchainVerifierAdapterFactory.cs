using Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters.Bitcoin;
using Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters.Ripple;
using NBitcoin;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters
{
    public class BlockchainVerifierAdapterFactory
    {
        public IBlockchainVerifierAdapter GetAdapter(string blockchainType, params string[] args)
        {
            switch (blockchainType)
            {
                case "Bitcoin":
                    return new BitcoinBlockchainVerifierAdapter(args[0], Network.GetNetwork(args[1]));

                case "Ripple":
                    return new RippleBlockchainVerifierAdapter(args[0]);

                default:
                    return null;
            }
        }
    }
}
