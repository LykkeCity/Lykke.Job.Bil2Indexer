using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    [Route("api/blockchains/{blockchainType}/transactions")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionQueryFacade _transactionQueryFacade;
        public TransactionsController(ITransactionQueryFacade transactionQueryFacade)
        {
            _transactionQueryFacade = transactionQueryFacade;
        }

        [HttpGet(Name = nameof(GetTransactions))]
        public async Task<ActionResult<Paginated<TransactionModel>>> GetTransactions(
            [FromRoute] string blockchainType,
            [FromQuery] string blockId,
            [FromQuery] int? blockNumber, 
            [FromQuery] string address,
            PaginationRequest pagination)
        {
            IReadOnlyCollection<TransactionModel> transactions;

            // TODO: Validate parameters

            if (blockId != null)
            {
                transactions = await _transactionQueryFacade.GetTransactionsByBlockId(blockchainType, 
                    blockId,
                    pagination.Limit,
                    pagination.Order == PaginationOrder.Asc,
                    pagination.StartingAfter,
                    pagination.EndingBefore);
            } 
            else if (blockNumber != null)
            {
                transactions = await _transactionQueryFacade.GetTransactionsByBlockNumber(blockchainType, 
                    blockNumber.Value,
                    pagination.Limit,
                    pagination.Order == PaginationOrder.Asc,
                    pagination.StartingAfter,
                    pagination.EndingBefore);
            }
            else if (address != null)
            {
                transactions = await _transactionQueryFacade.GetTransactionsByAddress(blockchainType, 
                    address,
                    pagination.Limit,
                    pagination.Order == PaginationOrder.Asc,
                    pagination.StartingAfter,
                    pagination.EndingBefore);
            }
            else
            {
                // TODO: Describe why request is failed
                return BadRequest();
            }

            return transactions.Paginate(pagination);
        }

        [HttpGet("{id}", Name = nameof(GetTransactionById))]
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
