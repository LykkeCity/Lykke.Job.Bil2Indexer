using System;
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

        [CanBeNull]
        public string SpentByTransactionId { get; private set; }

        private bool IsSpent => SpentByTransactionId != null;

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
            string spentByTransactionId)
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
            SpentByTransactionId = spentByTransactionId;
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
                spentByTransactionId: null
            );
        }

        public void SpendBy(string transactionId)
        {
            if (IsSpent)
            {
                EnsureSpentBy(transactionId);
            }

            SpentByTransactionId = transactionId;
        }

        public void RevertSpendingBy(string transactionId)
        {
            if (IsSpent)
            {
                EnsureSpentBy(transactionId);
            }

            SpentByTransactionId = null;
        }

        public override string ToString()
        {
            return IsSpent 
                ? $"{BlockchainType}:{Id.TransactionId}:{Id.CoinNumber} spent by {SpentByTransactionId}" 
                : $"{BlockchainType}:{Id.TransactionId}:{Id.CoinNumber} unspent";
        }

        private void EnsureSpentBy(string transactionId)
        {
            if(SpentByTransactionId != transactionId)
            {
                throw new InvalidOperationException($"Coin {this} can't be spent by transaction {transactionId}, because it already was spent by transaction {SpentByTransactionId}.");
            }
        }
    }
}
