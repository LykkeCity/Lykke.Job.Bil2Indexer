using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins
{
    public static class CoinIdBuilder
    {
        public static string BuildCoinId(this CoinId source)
        {
            return BuildCoinId(source.TransactionId, source.CoinNumber);
        }

        public static string BuildCoinId(string transactionId, int coinNumber)
        {
            return $"{transactionId}_{coinNumber}";
        }
    }
}
