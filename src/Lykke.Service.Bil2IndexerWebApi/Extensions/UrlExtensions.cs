using Lykke.Bil2.SharedDomain;
using Lykke.Service.Bil2IndexerWebApi.Controllers;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests.Shared;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Extensions
{
    public static class UrlExtensions
    {
        public static string RawTransactionUrl(this IUrlHelper url, string blockchainType,  TransactionId transactionId)
        {
            return url.Action(nameof(TransactionsController.GetTransactionRawById),
                    ControllerHelper.GetShortName<TransactionsController>(),
                    new ByIdRequest {BlockchainType = blockchainType, Id = transactionId});
        }

        public static string BlockTransactionUrl(this IUrlHelper url, string blockchainType, BlockId id)
        {
            return url.Action(nameof(TransactionsController.GetTransactions),
                ControllerHelper.GetShortName<TransactionsController>(),
                new TransactionsRequest { BlockchainType = blockchainType, BlockId = id});
        }


        public static string RawBlockUrl(this IUrlHelper url, string blockchainType, BlockId id)
        {
            return url.Action(nameof(BlocksController.GetRawBlock),
                ControllerHelper.GetShortName<BlocksController>(),
                new ByIdRequest { BlockchainType = blockchainType, Id = id });
        }

        public static string BlockUrl(this IUrlHelper url, string blockchainType, long height)
        {
            return url.Action(nameof(BlocksController.GetBlockByHeigh),
                ControllerHelper.GetShortName<BlocksController>(),
                new ByBlockNumberRequest { BlockchainType = blockchainType, Number = height });
        }

        public static string BlockUrl(this IUrlHelper url, string blockchainType, BlockId id)
        {
            return url.Action(nameof(BlocksController.GetBlockById),
                ControllerHelper.GetShortName<BlocksController>(),
                new ByIdRequest { BlockchainType = blockchainType, Id = id });
        }
    }
}
