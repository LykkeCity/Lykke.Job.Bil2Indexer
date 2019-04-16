using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers
{
    public class MoneyHelper
    {
        public static Money BuildMoney(string source, int scale)
        {
            return Money.Round(Money.Parse(source.Replace(",", ".")), scale);
        }
        public static UMoney BuildUMoney(string source, int scale)
        {
            return UMoney.Round(UMoney.Parse(source.Replace(",", ".")), scale);
        }
    }
}
