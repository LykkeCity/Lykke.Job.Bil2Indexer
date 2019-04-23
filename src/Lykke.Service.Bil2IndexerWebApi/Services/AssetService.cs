using System.Threading.Tasks;
using DataApi.Core.Domain;

namespace DataApi.Services
{
    public class AssetService : IAssetService
    {
        public Task<Asset> GetAsset(string address, string ticker)
        {
            throw new System.NotImplementedException();
        }

        public Task<Asset[]> GetAssets(int limit, bool orderAsc, string startingAfter, string endingBefore)
        {
            throw new System.NotImplementedException();
        }
    }
}
