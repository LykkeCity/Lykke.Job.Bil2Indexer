using System;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.BlockHeaders.Mappers
{
    internal static class BlockHeaderMappers
    {
        public static BlockState ToDomain(this BlockHeaderEntity.BlockState source)
        {
            switch (source)
            {
                case BlockHeaderEntity.BlockState.Assembling:
                    return BlockState.Assembling;
                case BlockHeaderEntity.BlockState.Assembled:
                    return BlockState.Assembled;
                case BlockHeaderEntity.BlockState.Executed:
                    return BlockState.Executed;
                case BlockHeaderEntity.BlockState.PartiallyExecuted:
                    return BlockState.PartiallyExecuted;
                case BlockHeaderEntity.BlockState.RolledBack:
                    return BlockState.RolledBack;
                default:
                    throw new ArgumentException("Invalid switch", nameof(source));
            }
        }

        public static BlockHeaderEntity.BlockState ToDbEntity(this BlockState source)
        {
            switch (source)
            {
                case BlockState.Assembling:
                    return BlockHeaderEntity.BlockState.Assembling;
                case BlockState.Assembled:
                    return BlockHeaderEntity.BlockState.Assembled;
                case BlockState.Executed:
                    return BlockHeaderEntity.BlockState.Executed;
                case BlockState.PartiallyExecuted:
                    return BlockHeaderEntity.BlockState.PartiallyExecuted;
                case BlockState.RolledBack:
                    return BlockHeaderEntity.BlockState.RolledBack;
                default:
                    throw new ArgumentException("Invalid switch", nameof(source));
            }
        }

        public static BlockHeaderEntity ToDbEntity(this BlockHeader source)
        {
            return new BlockHeaderEntity
            {
                Version = (uint)source.Version,
                State = source.State.ToDbEntity(),
                Id = source.Id,
                MinedAt = source.MinedAt,
                Number = source.Number,
                PreviousBlockId = source.PreviousBlockId,
                Size = source.Size,
                TransactionCount = source.TransactionsCount
            };
        }

        public static BlockHeader ToDomain(this BlockHeaderEntity source, string blockchainType)
        {
            return new BlockHeader(id: source.Id,
                version: source.Version,
                blockchainType: blockchainType,
                number: source.Number,
                minedAt: source.MinedAt,
                size: source.Size,
                transactionsCount: source.TransactionCount,
                previousBlockId: source.PreviousBlockId,
                state: source.State.ToDomain());
        }
    }
}
