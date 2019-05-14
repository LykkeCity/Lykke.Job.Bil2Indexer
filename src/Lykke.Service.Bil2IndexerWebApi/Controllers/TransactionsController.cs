using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
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
        private readonly ITransactionService _transactionService;
        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpGet(Name = nameof(GetTransactions))]
        public async Task<ActionResult<Paginated<TransactionModel>>> GetTransactions(
            [FromRoute] string blockchainType,
            [FromQuery] string blockId,
            [FromQuery] int? blockNumber, 
            [FromQuery] string address,
            [FromQuery] PaginationOrder order, 
            [FromQuery] string startingAfter,
            [FromQuery] string endingBefore, 
            [FromQuery] int limit = 25)
        {
            IReadOnlyCollection<Transaction> transactions;

            // TODO: Validate parameters

            if (blockId != null)
            {
                transactions = await _transactionService.GetTransactionsByBlockId(blockchainType, blockId, limit, order == PaginationOrder.Asc, startingAfter, endingBefore);
            } 
            else if (blockNumber != null)
            {
                transactions = await _transactionService.GetTransactionsByBlockNumber(blockchainType, blockNumber.Value, limit, order == PaginationOrder.Asc, startingAfter, endingBefore);
            }
            else if (address != null)
            {
                transactions = await _transactionService.GetTransactionsByAddress(blockchainType, address, limit, order == PaginationOrder.Asc, startingAfter, endingBefore);
            }
            else
            {
                // TODO: Describe why request is failed
                return BadRequest();
            }

            var model = TransactionModelMapper.Map(transactions);

            return model;
        }

        [HttpGet("{id}", Name = nameof(GetTransactionById))]
        public async Task<ActionResult<TransactionModel>> GetTransactionById(
            [FromRoute] string blockchainType,
            [FromRoute] string id)
        {
            var transaction = await _transactionService.GetTransactionById(blockchainType, id);

            if (transaction == null)
            {
                return NotFound();
            }

            var model = TransactionModelMapper.Map(transaction);

            return model;
        }
    }
}
