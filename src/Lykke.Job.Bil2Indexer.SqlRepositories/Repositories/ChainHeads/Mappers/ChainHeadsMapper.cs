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
                source.CrawlerSequence,
                source.BlockNumber,
                source.BlockId,
                source.PreviousBlockId,
                source.Mode
            );
        }

        public static ChainHeadEntity ToDbEntity(this ChainHead source)
        {
            return new ChainHeadEntity
            {
                Version = (uint)source.Version,
                Sequence = source.Sequence,
                CrawlerSequence = source.CrawlerSequence,
                Id = source.BlockchainType,
                FirstBlockNumber = source.FirstBlockNumber,
                BlockId = source.BlockId,
                PreviousBlockId = source.PreviousBlockId,
                BlockNumber = source.BlockNumber,
                Mode = source.Mode
            };
        }
    }
}
