using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    public class TransactionsController : ControllerBase
    {
        private const string RoutePrefix = "api/blockchains/{blockchainType}/transactions";

        private readonly ITransactionQueryFacade _transactionQueryFacade;
        public TransactionsController(ITransactionQueryFacade transactionQueryFacade)
        {
            _transactionQueryFacade = transactionQueryFacade;
        }

        [HttpGet(RoutePrefix, Name = nameof(GetTransactions))]
        public async Task<ActionResult<Paginated<TransactionModel, string>>> GetTransactions([FromRoute][FromQuery] TransactionsRequest request)
        {
            IReadOnlyCollection<TransactionModel> transactions;

            // TODO: Validate parameters

            if (request.BlockId != null)
            {
                transactions = await _transactionQueryFacade.GetTransactionsByBlockId(request.BlockchainType,
                    request.BlockId,
                    request.Pagination.Limit,
                    request.Pagination.Order == PaginationOrder.Asc,
                    request.Pagination.StartingAfter,
                    request.Pagination.EndingBefore);
            } 
            else if (request.BlockNumber != null)
            {
                transactions = await _transactionQueryFacade.GetTransactionsByBlockNumber(request.BlockchainType,
                    request.BlockNumber.Value,
                    request.Pagination.Limit,
                    request.Pagination.Order == PaginationOrder.Asc,
                    request.Pagination.StartingAfter,
                    request.Pagination.EndingBefore);
            }
            else if (request.Address != null)
            {
                transactions = await _transactionQueryFacade.GetTransactionsByAddress(request.BlockchainType,
                    request.Address,
                    request.Pagination.Limit,
                    request.Pagination.Order == PaginationOrder.Asc,
                    request.Pagination.StartingAfter,
                    request.Pagination.EndingBefore);
            }
            else
            {
                throw new ArgumentException("This should not happen due validation logic");
            }

            return transactions.Paginate(request.Pagination);
        }

        [HttpGet(RoutePrefix + "{id}", Name = nameof(GetTransactionById))]
        public async Task<ActionResult<TransactionModel>> GetTransactionById(
            [FromRoute] string blockchainType,
            [FromRoute] string id)
        {
            var transaction = await _transactionQueryFacade.GetTransactionById(blockchainType, id);

            if (transaction == null)
            {
                return NotFound();
            }

            return transaction;
        }
    }
}
