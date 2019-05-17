using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class Coin
    {
        public string BlockchainType { get; }

        public CoinId Id { get; }

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

        public bool IsSpent { get; }

        public BlockId BlockId { get; }

        public long BlockNumber { get; }

        public Coin(
            string blockchainType,
            CoinId id,
            Asset asset,
            UMoney value,
            Address address,
            AddressTag addressTag,
            AddressTagType? addressTagType,
            long? addressNonce,
            bool isSpent,
            BlockId blockId,
            long blockNumber)
        {
            BlockchainType = blockchainType;
            Id = id;
            Asset = asset;
            Value = value;
            Address = address;
            AddressTag = addressTag;
            AddressTagType = addressTagType;
            AddressNonce = addressNonce;
            IsSpent = isSpent;
            BlockId = blockId;
            BlockNumber = blockNumber;
        }

        public static Coin CreateUnspent(
            string blockchainType,
            CoinId id,
            Asset asset,
            UMoney value,
            Address address,
            AddressTag addressTag,
            AddressTagType? addressTagType,
            long? addressNonce,
            BlockId blockId,
            long blockNumber)
        {
            return new Coin
            (
                blockchainType: blockchainType,
                id: id,
                asset: asset,
                value: value,
                address: address,
                addressTag: addressTag,
                addressTagType: addressTagType,
                addressNonce: addressNonce,
                isSpent: false,
                blockId: blockId,
                blockNumber: blockNumber
            );
        }

        public override string ToString()
        {
            return IsSpent 
                ? $"{BlockchainType}:{Id} spent" 
                : $"{BlockchainType}:{Id} unspent";
        }
    }
}
