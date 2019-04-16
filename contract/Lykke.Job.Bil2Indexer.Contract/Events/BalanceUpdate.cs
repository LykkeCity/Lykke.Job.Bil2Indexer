using System;
using JetBrains.Annotations;
using Lykke.Bil2.SharedDomain;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Contract.Events
{
    [PublicAPI]
    public class BalanceUpdate
    {
        public AccountId AccountId { get; }
        public Money OldBalance { get; }
        public Money NewBalance { get; }
        [CanBeNull]
        public string TransferId { get; }
        [CanBeNull]
        public int? CoinNumber { get; }
        [CanBeNull]
        public AddressTag AddressTag { get; }
        [CanBeNull]
        public AddressTagType? AddressTagType { get; }
        [CanBeNull]
        public long? Nonce { get; }
        
        public BalanceUpdate(
            AccountId accountId,
            Money oldBalance,
            Money newBalance,
            [CanBeNull] string transferId,
            [CanBeNull] int? coinNumber,
            [CanBeNull] AddressTag addressTag,
            [CanBeNull] AddressTagType? addressTagType,
            [CanBeNull] long? nonce)
        {
            AccountId = AccountId ?? throw new ArgumentNullException(nameof(accountId));
            OldBalance = oldBalance;
            NewBalance = newBalance;
            TransferId = transferId;
            CoinNumber = coinNumber;
            AddressTag = addressTag;
            AddressTagType = addressTagType;
            Nonce = nonce;
        }
    }
}
