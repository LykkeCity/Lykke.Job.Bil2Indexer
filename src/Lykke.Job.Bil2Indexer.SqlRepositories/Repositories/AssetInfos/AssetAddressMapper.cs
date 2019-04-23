using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.AssetInfos
{
    public static class AssetAddressMapper
    {
        private const string DbNullMagicValue = "<null>";

        public static string ToDbString(this AssetAddress source)
        {
            return !string.IsNullOrEmpty(source?.Value) ? source.Value : DbNullMagicValue;
        }

        public static AssetAddress FromDbString(string source)
        {
            return !string.IsNullOrEmpty(source) ? new AssetAddress(source) : null;
        }
    }
}
