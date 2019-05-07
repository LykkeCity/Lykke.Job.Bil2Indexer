using System;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Helpers;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Coins.Mappers
{
    internal static class CoinDbEntityMapper
    {
        public static CoinEntity ToDbEntity(this Coin source)
        {
            return new CoinEntity
            {
                TransactionId = source.Id.TransactionId,
                CoinNumber = source.Id.CoinNumber,
                Address = source.Address,
                AddressNonce = source.AddressNonce,
                AddressTag = source.AddressTag,
                AddressTagType = ToDbEntity(source.AddressTagType),
                AssetAddress = source.Asset.Address,
                IsSpent = source.IsSpent,
                AssetId = source.Asset.Id,
                ValueScale = source.Value.Scale,
                ValueString = MoneyHelper.BuildPgString(source.Value),
                CoinId = CoinIdBuilder.BuildCoinId(source.Id)
            };
        }

        public static Coin ToDomain(this CoinEntity source, string blockchainType)
        {
            return new Coin(blockchainType: blockchainType,
                id: new CoinId(source.TransactionId, source.CoinNumber),
                asset: new Asset(new AssetId(source.AssetId), source.AssetAddress != null ? new AssetAddress(source.AssetAddress) : null),
                address: source.Address,
                value: MoneyHelper.BuildUMoney(source.ValueString, source.ValueScale),
                addressNonce: source.AddressNonce,
                addressTag: source.AddressTag,
                addressTagType: ToDomain(source.AddressTagType),
                isSpent: source.IsSpent);
        }


        private static AddressTagType? ToDomain(CoinEntity.AddressTagTypeValues? source)
        {
            if (source == null)
            {
                return null;
            }

            switch (source.Value)
            {
                case CoinEntity.AddressTagTypeValues.Number:
                    return AddressTagType.Number;
                case CoinEntity.AddressTagTypeValues.Text:
                    return AddressTagType.Text;
                default:
                    throw new ArgumentException("Unknown mapping", nameof(source));
            }
        }

        private static CoinEntity.AddressTagTypeValues? ToDbEntity(AddressTagType? source)
        {
            if (source == null)
            {
                return null;
            }

            switch (source.Value)
            {
                case AddressTagType.Number:
                    return CoinEntity.AddressTagTypeValues.Number;
                case AddressTagType.Text:
                    return CoinEntity.AddressTagTypeValues.Text;
                default:
                    throw new ArgumentException($"Unknown mapping", nameof(source));
            }
        }
    }
}
