using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.FeeEnvelopes
{
    public static class FeeEnvelopePredicates
    {
        public static Expression<Func<FeeEnvelopeEntity, bool>> Build(string blockchainType, BlockId blockId)
        {
            var stringBlockId = blockId.ToString();

            return p => p.BlockchainType == blockchainType && p.BlockId == stringBlockId;
        }

        public static Expression<Func<FeeEnvelopeEntity, bool>> Build(string blockchainType, TransactionId transactionId)
        {
            var stringTransactionId = transactionId.ToString();

            return p => p.BlockchainType == blockchainType
                        && p.TransactionId == stringTransactionId;

        }

        public static Expression<Func<FeeEnvelopeEntity, bool>> Build(string blockchainType, TransactionId transactionId, Asset asset)
        {
            var stringTransactionId = transactionId.ToString();

            var stringAssetId = asset.Id.ToString();
            var stringAssetAddress = asset.Address?.ToString();

            //force to use filtered index
            if (stringAssetAddress != null)
            {
                return p => p.AssetAddress != null
                            && p.BlockchainType == blockchainType
                            && p.TransactionId == stringTransactionId
                            && p.AssetId == stringAssetId
                            && p.AssetAddress == stringAssetAddress;
            }
            else
            {
                return p => p.AssetAddress == null
                            && p.BlockchainType == blockchainType
                            && p.TransactionId == stringTransactionId
                            && p.AssetId == stringAssetId;
            }
        }

        public static Expression<Func<FeeEnvelopeEntity, bool>> Build(string blockchainType, IEnumerable<string> transactionIds, bool? isAssetAddressNull = null)
        {
            if (isAssetAddressNull == null)
            {
                return dbEntity => dbEntity.BlockchainType == blockchainType
                                   && transactionIds.Contains(dbEntity.TransactionId);
            }

            if (isAssetAddressNull.Value)
            {
                return dbEntity => dbEntity.AssetAddress == null
                                   && dbEntity.BlockchainType == blockchainType
                                   && transactionIds.Contains(dbEntity.TransactionId);
            }
            else
            {
                return dbEntity => dbEntity.AssetAddress != null
                                   && dbEntity.BlockchainType == blockchainType
                                   && transactionIds.Contains(dbEntity.TransactionId);
            }
        }
    }
}
