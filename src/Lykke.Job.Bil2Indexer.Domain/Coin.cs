using JetBrains.Annotations;
using Lykke.Bil2.Contract.Common;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class Coin
    {
        public string BlockchainType { get; }

        public CoinReference Id { get; }

        public long Version { get; }

        public Asset Asset { get; }

        public UMoney Value { get; }

        [CanBeNull]
        public Address Address { get; }

        [CanBeNull]
        public AddressTag AddressTag { get; }

        [CanBeNull]
        public AddressTagType? AddressTagType { get; }
        
        [CanBeNull]
        public long? AddressNonce { get; }

        public bool IsSpent { get; private set; }

        public Coin(
            string blockchainType,
            CoinReference id,
            long version,
            Asset asset,
            UMoney value,
            Address address,
            AddressTag addressTag,
            AddressTagType? addressTagType,
            long? addressNonce,
            bool isSpent)
        {
            BlockchainType = blockchainType;
            Id = id;
            Version = version;
            Asset = asset;
            Value = value;
            Address = address;
            AddressTag = addressTag;
            AddressTagType = addressTagType;
            AddressNonce = addressNonce;
            IsSpent = isSpent;
        }

        public static Coin CreateUnspent(
            string blockchainType,
            CoinReference id,
            Asset asset,
            UMoney value,
            Address address,
            AddressTag addressTag,
            AddressTagType? addressTagType,
            long? addressNonce)
        {
            return new Coin
            (
                blockchainType: blockchainType,
                id: id,
                version: 0,
                asset: asset,
                value: value,
                address: address,
                addressTag: addressTag,
                addressTagType: addressTagType,
                addressNonce: addressNonce,
                isSpent: false
            );
        }

        public override string ToString()
        {
            return IsSpent 
                ? $"{BlockchainType}:{Id.TransactionId}:{Id.CoinNumber} spent" 
                : $"{BlockchainType}:{Id.TransactionId}:{Id.CoinNumber} unspent";
        }
    }
}
