using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Factories;
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
        private readonly ITransactionModelFactory _transactionModelFactory;

        public TransactionsController(ITransactionService transactionService, ITransactionModelFactory transactionModelFactory)
        {
            _transactionService = transactionService;
            _transactionModelFactory = transactionModelFactory;
        }

        [HttpGet(Name = nameof(GetTransactions))]
        public async Task<ActionResult<Paginated<TransactionModel[]>>> GetTransactions(
            [FromRoute] string blockchainType,
            [FromQuery] string blockId,
            [FromQuery] int? blockNumber, 
            [FromQuery] string address,
            PaginationOrder order, 
            string startingAfter,
            string endingBefore, 
            int limit = 25)
        {
            Transaction[] transactions = null;

            if (blockId != null)
            {
                transactions =
                    await _transactionService.GetTransactionsByBlockId(blockId, limit, order == PaginationOrder.Asc, startingAfter, endingBefore);
            }

            if (blockNumber != null)
            {
                transactions =
                    await _transactionService.GetTransactionsByBlockNumber(blockNumber.Value, limit, order == PaginationOrder.Asc, startingAfter, endingBefore);
            }

            if (address != null)
            {
                transactions =
                    await _transactionService.GetTransactionsByAddress(address, limit, order == PaginationOrder.Asc, startingAfter, endingBefore);
            }

            var model = _transactionModelFactory.PrepareTransactionsPaginated(transactions);

            return model;
        }

        [HttpGet("{id}", Name = nameof(GetTransactionById))]
        public async Task<ActionResult<TransactionModel>> GetTransactionById(
            [FromRoute] string blockchainType,
            [FromRoute] string id)
        {
            var transaction = await _transactionService.GetTransactionById(id);

            var model = _transactionModelFactory.PrepareTransactionModel(transaction);

            return model;
        }
    }
}
