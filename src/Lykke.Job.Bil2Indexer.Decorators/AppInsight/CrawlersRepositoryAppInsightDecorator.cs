using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Domain.Services.Infrastructure;

namespace Lykke.Job.Bil2Indexer.Decorators.AppInsight
{
    [UsedImplicitly]
    public class CrawlersRepositoryAppInsightDecorator : ICrawlersRepository
    {
        private readonly ICrawlersRepository _impl;
        private readonly IAppInsightTelemetryProvider _appInsightTelemetryProvider;

        public CrawlersRepositoryAppInsightDecorator(ICrawlersRepository crawlersRepository,
            IAppInsightTelemetryProvider appInsightTelemetryProvider)
        {
            _impl = crawlersRepository;
            _appInsightTelemetryProvider = appInsightTelemetryProvider;
        }

        public Task<Crawler> GetOrDefaultAsync(string blockchainType, CrawlerConfiguration configuration)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{blockchainType}-{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAndReturnAsync(operationName,
                operationId,
                async () => await _impl.GetOrDefaultAsync(blockchainType, configuration));
        }

        public Task SaveAsync(Crawler crawler)
        {
            var operationName = _appInsightTelemetryProvider.FormatOperationName(_impl);
            var operationId = $"{crawler.BlockchainType}-{Guid.NewGuid()}";

            return _appInsightTelemetryProvider.ExecuteMethodWithTelemetryAsync(operationName,
                operationId,
                async () => await _impl.SaveAsync(crawler));
        }
    }
}
