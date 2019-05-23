using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes
{
    internal static class FeeEnvelopePredicates
    {
        public static Expression<Func<FeeEnvelopeEntity, bool>> Build(BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return p => p.BlockId == stringBlockId;
        }

        public static Expression<Func<FeeEnvelopeEntity, bool>> Build(TransactionId transactionId)
        {
            var stringTransactionId = transactionId.ToString();

            return p => p.TransactionId == stringTransactionId && (p.AssetAddress == null || p.AssetAddress != null);
        }
        public static Expression<Func<FeeEnvelopeEntity, bool>> Build(TransactionId transactionId, Asset asset)
        {
            var stringTransactionId = transactionId.ToString();

            var stringAssetId = asset.Id.ToString();
            var stringAssetAddress = asset.Address?.ToString();

            //force to use filtered index
            if (stringAssetAddress != null)
            {
                return p => p.AssetAddress != null
                            && p.TransactionId == stringTransactionId
                            && p.AssetId == stringAssetId
                            && p.AssetAddress == stringAssetAddress;
            }
            else
            {
                return p => p.AssetAddress == null
                            && p.TransactionId == stringTransactionId
                            && p.AssetId == stringAssetId;
            }
        }

        public static Expression<Func<FeeEnvelopeEntity, bool>> Build(IEnumerable<TransactionId> transactionIds, bool? isAssetAddressNull = null)
        {
            var stringValues = transactionIds.Select(p => p.ToString());
            if (isAssetAddressNull == null)
            {
                return dbEntity => (dbEntity.AssetAddress == null || dbEntity.AssetAddress != null) //force to use index intersection instead of bitmap
                                   && stringValues.Contains(dbEntity.TransactionId);
            }

            if (isAssetAddressNull.Value)
            {
                return dbEntity => dbEntity.AssetAddress == null
                                   && stringValues.Contains(dbEntity.TransactionId);
            }
            else
            {
                return dbEntity => dbEntity.AssetAddress != null
                                   && stringValues.Contains(dbEntity.TransactionId);
            }
        }
    }
}
