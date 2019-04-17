using System;
using System.Collections.Generic;
using JetBrains.Annotations;
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
        public IReadOnlyCollection<Transfer> Transfers { get; }
        [CanBeNull]
        public IReadOnlyCollection<SpentCoin> SpentCoins { get; }
        [CanBeNull]
        public IReadOnlyCollection<ReceivedCoin> ReceivedCoins { get; }
        
        public BalanceUpdate(
            AccountId accountId,
            Money oldBalance,
            Money newBalance,
            [CanBeNull] IReadOnlyCollection<Transfer> transfers,
            [CanBeNull] IReadOnlyCollection<SpentCoin> spentCoins,
            [CanBeNull] IReadOnlyCollection<ReceivedCoin> receivedCoins)
        {
            AccountId = AccountId ?? throw new ArgumentNullException(nameof(accountId));
            OldBalance = oldBalance;
            NewBalance = newBalance;
            Transfers = transfers;
            SpentCoins = spentCoins;
            ReceivedCoins = receivedCoins;
        }
    }
}
