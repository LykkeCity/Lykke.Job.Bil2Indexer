﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface ICrawlersRepository
    {
        Task<Crawler> GetOrDefaultAsync(string blockchainType, CrawlerConfiguration configuration);
        Task SaveAsync(Crawler crawler);
        Task<IReadOnlyCollection<Crawler>> GetAllAsync(string blockchainType, IEnumerable<CrawlerConfiguration> configurations);
    }
}
