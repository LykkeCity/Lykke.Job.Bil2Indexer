using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    public static class ReceivedCoinMapper
    {
        public static ReceivedCoinEntity ToDbEntity(this ReceivedCoin source)
        {
            return new ReceivedCoinEntity
            {
                Address = source.Address,
                Asset = source.Asset,
                Value = source.Value,
                AddressNonce = source.AddressNonce,
                AddressTag = source.AddressTag,
                AddressTagType = source.AddressTagType,
                CoinNumber = source.CoinNumber
            };
        }

        public static ReceivedCoin ToDomain(this ReceivedCoinEntity source)
        {
            return new ReceivedCoin(coinNumber: source.CoinNumber,
                asset: source.Asset,
                value: source.Value,
                address: source.Address,
                addressTagType: source.AddressTagType,
                addressTag: source.AddressTag,
                addressNonce: source.AddressNonce);
        }
    }
}
