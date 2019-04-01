using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Job.Bil2Indexer.Controllers
{
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly ICrawlersRepository _crawlersRepository;
        private readonly ICoinsRepository _coinsRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly ITransactionsRepository _transactionsRepository;

        public MonitoringController(
            IBlockHeadersRepository blockHeadersRepository,
            ICrawlersRepository crawlersRepository,
            ICoinsRepository coinsRepository,
            IBalanceActionsRepository balanceActionsRepository,
            ITransactionsRepository transactionsRepository)
        {
            _blockHeadersRepository = blockHeadersRepository;
            _crawlersRepository = crawlersRepository;
            _coinsRepository = coinsRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _transactionsRepository = transactionsRepository;
        }

        [HttpGet]
        public IActionResult Check()
        {
            return Ok();
        }
    }
}
