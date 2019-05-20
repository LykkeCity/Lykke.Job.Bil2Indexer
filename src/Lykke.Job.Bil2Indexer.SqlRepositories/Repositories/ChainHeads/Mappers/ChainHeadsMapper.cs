using System;
using System.Collections.Generic;
using System.Text;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.IndexerState.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.ChainHeads.Mappers
{
    internal static class ChainHeadsMapper
    {
        public static ChainHead ToDomain(this ChainHeadEntity source, string blockchainType)
        {
            return new ChainHead
            (
                blockchainType,
                source.FirstBlockNumber,
                source.Version,
                source.Sequence,
                source.BlockNumber,
                source.BlockId,
                source.PreviousBlockId
            );
        }

        public static ChainHeadEntity ToDbEntity(this ChainHead source)
        {
            return new ChainHeadEntity
            {
                Version = (uint)source.Version,
                Sequence = source.Sequence,
                Id = source.BlockchainType,
                FirstBlockNumber = source.FirstBlockNumber,
                BlockId = source.BlockId,
                PreviousBlockId = source.PreviousBlockId,
                BlockNumber = source.BlockNumber
            };
        }
    }
}
