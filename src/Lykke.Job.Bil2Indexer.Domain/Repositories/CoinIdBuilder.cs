using System;
using System.Linq;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public static class CoinIdBuilder
    {
        private const string Delimenator = "_";
        public static string BuildCoinId(this CoinId source)
        {
            return BuildCoinId(source.TransactionId, source.CoinNumber);
        }

        public static string BuildCoinId(string transactionId, int coinNumber)
        {
            return $"{transactionId}{Delimenator}{coinNumber}";
        }

        public static CoinId BuildDomainOrDefault(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }
            
            var segments = source.Split(Delimenator);

            var coinNumber = -1;
            if (segments.Length != 2 && ! int.TryParse(segments.Skip(1).SingleOrDefault(), out coinNumber))
            {

                throw new ArgumentException(nameof(source));
            }

            return new CoinId(segments.Take(1).First(), coinNumber);
        }
    }
}
