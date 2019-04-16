using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class AssetInfo
    {
        public string BlockchainType { get; }
        public AssetId Id { get; }
        public int Scale { get; }

        public AssetInfo(string blockchainType, AssetId id, int scale)
        {
            BlockchainType = blockchainType;
            Id = id;
            Scale = scale;
        }
    }
}