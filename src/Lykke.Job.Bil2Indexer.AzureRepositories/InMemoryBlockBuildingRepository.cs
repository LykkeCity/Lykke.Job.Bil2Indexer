using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Job.Bil2Indexer.Domain.Repositories;

namespace Lykke.Job.Bil2Indexer.AzureRepositories
{
    public class InMemoryBlockBuildingRepository : IBlockBuildingsRepository
    {
        private readonly ConcurrentDictionary<string, BlockBuilding> _storage;

        public InMemoryBlockBuildingRepository()
        {
            _storage = new ConcurrentDictionary<string, BlockBuilding>();
        }

        public IReadOnlyCollection<BlockBuilding> GetAll()
        {
            return _storage.Values.ToArray();
        }
    }
}
