using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Lykke.Numerics;

namespace Lykke.Job.Bil2Indexer.Contract
{
    [PublicAPI]
    [DataContract]
    public class BalanceUpdate
    {
        [DataMember(Order = 0)]
        public AccountId AccountId { get; }
        
        [DataMember(Order = 1)]
        public Money OldBalance { get; }
        
        [DataMember(Order = 2)]
        public Money NewBalance { get; }

        [CanBeNull]
        [DataMember(Order = 3)]
        public IReadOnlyCollection<Transfer> Transfers { get; }
        
        [CanBeNull]
        [DataMember(Order = 4)]
        public IReadOnlyCollection<SpentCoin> SpentCoins { get; }
        
        [CanBeNull]
        [DataMember(Order = 5)]
        public IReadOnlyCollection<ReceivedCoin> ReceivedCoins { get; }
        
        public BalanceUpdate(
            AccountId accountId,
            Money oldBalance,
            Money newBalance,
            [CanBeNull] IReadOnlyCollection<Transfer> transfers,
            [CanBeNull] IReadOnlyCollection<SpentCoin> spentCoins,
            [CanBeNull] IReadOnlyCollection<ReceivedCoin> receivedCoins)
        {
            AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
            OldBalance = oldBalance;
            NewBalance = newBalance;
            Transfers = transfers;
            SpentCoins = spentCoins;
            ReceivedCoins = receivedCoins;
        }
    }
}
