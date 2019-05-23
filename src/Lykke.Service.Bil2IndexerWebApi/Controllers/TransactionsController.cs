using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests.Shared;
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
        public async Task<ActionResult<Paginated<TransactionResponce, string>>> GetTransactions(
            [FromRoute][FromQuery] TransactionsRequest request)
        {
            IReadOnlyCollection<TransactionResponce> transactions;
            
            if (request.BlockId != null)
            {
                transactions = await _transactionQueryFacade.GetTransactionsByBlockId(request.BlockchainType,
                    request.BlockId,
                    request.Limit,
                    request.Order == PaginationOrder.Asc,
                    request.StartingAfter,
                    request.EndingBefore, 
                    Url);
            } 
            else if (request.BlockNumber != null)
            {
                transactions = await _transactionQueryFacade.GetTransactionsByBlockNumber(request.BlockchainType,
                    request.BlockNumber.Value,
                    request.Limit,
                    request.Order == PaginationOrder.Asc,
                    request.StartingAfter,
                    request.EndingBefore, 
                    Url);
            }
            else if (request.Address != null)
            {
                transactions = await _transactionQueryFacade.GetTransactionsByAddress(request.BlockchainType,
                    request.Address,
                    request.Limit,
                    request.Order == PaginationOrder.Asc,
                    request.StartingAfter,
                    request.EndingBefore,
                    Url);
            }
            else
            {
                throw new ArgumentException("This should not happen due validation logic");
            }

            return transactions.Paginate(request, Url, p => p.Id);
        }

        [HttpGet(RoutePrefix + "/{id}", Name = nameof(GetTransactionById))]
        public async Task<ActionResult<TransactionResponce>> GetTransactionById(
            [FromRoute] ByIdRequest request)
        {
            return await _transactionQueryFacade.GetTransactionById(request.BlockchainType, request.Id, Url);
        }

        [HttpGet(RoutePrefix + "/{id}/raw", Name = nameof(GetTransactionRawById))]
        public Task<ActionResult<TransactionResponce>> GetTransactionRawById(
            [FromRoute] ByIdRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
