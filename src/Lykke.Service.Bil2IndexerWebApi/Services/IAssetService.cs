using DataApi.Core.Domain;
using System.Threading.Tasks;

namespace DataApi.Services
{
    public interface IAssetService
    {
        Task<Asset> GetAsset(string address, string ticker);
        Task<Asset[]> GetAssets(int limit, bool orderAsc, string startingAfter,
            string endingBefore);
    }
}
