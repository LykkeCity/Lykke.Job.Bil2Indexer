using System;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.Decorators
{
    public class CrawlersDecoratorRepository : ICrawlersRepository
    {
        private readonly ICrawlersRepository _crawlersRepository;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public CrawlersDecoratorRepository(ICrawlersRepository crawlersRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _crawlersRepository = crawlersRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public async Task<Crawler> GetOrDefaultAsync(string blockchainType, CrawlerConfiguration configuration)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(CrawlersDecoratorRepository),
                nameof(GetOrDefaultAsync));
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _crawlersRepository.GetOrDefaultAsync(blockchainType, configuration));
        }

        public async Task SaveAsync(Crawler crawler)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(nameof(CrawlersDecoratorRepository),
                nameof(SaveAsync));
            var operationId = $"{crawler.BlockchainType}-{Guid.NewGuid()}";

            await _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _crawlersRepository.SaveAsync(crawler));
        }
    }
}
