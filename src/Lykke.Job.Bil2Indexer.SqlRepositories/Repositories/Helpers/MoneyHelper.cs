using System;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers
{
    public class MoneyHelper
    {
        public static Money BuildMoney(string source, int scale)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source));
            }

            return Money.Round(Money.Parse(source.Replace(",", ".")), scale);
        }

        public static UMoney BuildUMoney(string source, int scale)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentNullException(nameof(source));
            }

            return UMoney.Round(UMoney.Parse(source.Replace(",", ".")), scale);
        }

        public static string BuildPgString(Money source)
        {
            return source.ToString().Replace(",", ".");
        }

        public static string BuildPgString(UMoney source)
        {
            return source.ToString().Replace(",", ".");
        }
    }
}
