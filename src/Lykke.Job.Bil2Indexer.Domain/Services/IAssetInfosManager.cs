using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services
{
    public interface IAssetInfosManager
    {
        Task EnsureAdded(ISet<AssetInfo> assets);
    }
}
