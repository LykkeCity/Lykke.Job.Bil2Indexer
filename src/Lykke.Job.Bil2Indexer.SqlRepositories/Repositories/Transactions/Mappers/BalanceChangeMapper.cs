using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Blockchain.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models;
using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess.Transactions.Models.Props;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.Repositories.Transactions.Mappers
{
    public static class BalanceChangeMapper
    {
        public static BalanceChangeEntity ToDbEntity(this BalanceChange source)
        {
            return new BalanceChangeEntity
            {
                Address = source.Address,
                Asset = source.Asset,
                Nonce = source.Nonce,
                Tag = source.Tag,
                TagType = source.TagType,
                TransferId = source.TransferId,
                Value = source.Value
            };
        }

        public static BalanceChange ToDomain(this BalanceChangeEntity source)
        {
            return new BalanceChange(transferId:source.TransferId,
                asset: source.Asset, 
                value:source.Value,
                address: source.Address,
                tag: source.Tag, 
                tagType:source.TagType,
                nonce:source.Nonce);
        }
    }
}
