using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    public static class BlockModelMapper
    {
        public static BlockResponce ToViewModel(this BlockHeader source, long lastBlockNumber)
        {
            return new BlockResponce
            {
                Id = source.Id,
                Number = source.Number,
                ConfirmationsCount = lastBlockNumber - source.Number,
                Size = source.Size,
                MinedAt = source.MinedAt,
                TransactionsCount = source.TransactionsCount,
                PrevBlockId = source.PreviousBlockId,
                //TODO
                IsIrreversible = true,
                //TODO
                Links = null
            };
        }

        public static IReadOnlyCollection<BlockResponce> ToViewModel(this IReadOnlyCollection<BlockHeader> source, long lastBlockNumber)
        {
            return source.Select(p => p.ToViewModel(lastBlockNumber)).ToList();
        }
    }
}
