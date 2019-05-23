using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Extensions;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Mappers
{
    internal static class BlockModelMapper
    {
        public static BlockResponce ToViewModel(this BlockHeader source, long lastBlockNumber, IUrlHelper url, string blockchainType)
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
                Links = new BlockLinks
                {
                    RawUrl = url.RawBlockUrl(blockchainType, source.Id),
                    PrevBlockUrl = source.PreviousBlockId != null ? url.BlockUrl(blockchainType, source.PreviousBlockId) : null,
                    NextBlockUrl = source.Number < lastBlockNumber ? url.BlockUrl(blockchainType, source.Number + 1) : null,
                    TransactionsUrl = url.BlockTransactionUrl(blockchainType, source.Id)
                },
                IsIrreversible = true,
            };
        }

        public static IReadOnlyCollection<BlockResponce> ToViewModel(this IReadOnlyCollection<BlockHeader> source, long lastBlockNumber, IUrlHelper url, string blockchainType)
        {
            return source.Select(p => p.ToViewModel(lastBlockNumber, url, blockchainType)).ToList();
        }
    }
}
