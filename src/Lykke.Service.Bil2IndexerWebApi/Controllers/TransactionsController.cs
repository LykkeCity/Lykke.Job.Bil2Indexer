using System.Threading.Tasks;
using DataApi.Core.Domain;
using DataApi.Factories;
using DataApi.Models;
using DataApi.Models.Common;
using DataApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet("{id}", Name = nameof(GetTransactionById))]
        public async Task<ActionResult<TransactionModel>> GetTransactionById([FromRoute] string id)
        {
            var transaction = await _transactionService.GetTransactionById(id);

            var model = _transactionModelFactory.PrepareTransactionModel(transaction);

            return model;
        }

        [HttpGet(Name = nameof(GetTransactions))]
        public async Task<ActionResult<Paginated<TransactionModel[]>>> GetTransactions([FromRoute] string blockId,
            [FromRoute] int? blockNumber, [FromRoute] string address,
            PaginationOrder order, string startingAfter, string endingBefore, int limit = 25)
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
    }
}