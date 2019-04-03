using System.Linq;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Job.Bil2Indexer.Controllers
{
    [Route("api/[controller]")]
    public class MonitoringController : ControllerBase
    {
        private readonly IntegrationSettingsProvider _settingsProvider;
        private readonly IBlockHeadersRepository _blockHeadersRepository;
        private readonly ICrawlersRepository _crawlersRepository;
        private readonly ICoinsRepository _coinsRepository;
        private readonly IBalanceActionsRepository _balanceActionsRepository;
        private readonly ITransactionsRepository _transactionsRepository;
        private readonly IChainHeadsRepository _chainHeadsRepository;

        public MonitoringController(
            IntegrationSettingsProvider settingsProvider,
            IBlockHeadersRepository blockHeadersRepository,
            ICrawlersRepository crawlersRepository,
            ICoinsRepository coinsRepository,
            IBalanceActionsRepository balanceActionsRepository,
            ITransactionsRepository transactionsRepository,
            IChainHeadsRepository chainHeadsRepository)
        {
            _settingsProvider = settingsProvider;
            _blockHeadersRepository = blockHeadersRepository;
            _crawlersRepository = crawlersRepository;
            _coinsRepository = coinsRepository;
            _balanceActionsRepository = balanceActionsRepository;
            _transactionsRepository = transactionsRepository;
            _chainHeadsRepository = chainHeadsRepository;
        }

        [HttpGet]
        public IActionResult Check()
        {
            var response = _settingsProvider.GetAll()
                .Select(x => new
                {
                    BlockchainType = x.Key,
                    Head = _chainHeadsRepository.GetAsync(x.Key).ConfigureAwait(false).GetAwaiter().GetResult()
                });

            return Ok(response);
        }
    }
}
