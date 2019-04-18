using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class AssetInfo
    {
        public string BlockchainType { get; }
        public Asset Asset { get; }
        public int Scale { get; }

        public AssetInfo(string blockchainType, Asset asset, int scale)
        {
            BlockchainType = blockchainType;
            Asset = asset;
            Scale = scale;
        }
    }
}
