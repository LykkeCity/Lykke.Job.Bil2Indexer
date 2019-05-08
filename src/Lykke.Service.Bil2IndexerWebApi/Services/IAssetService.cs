using System.Threading.Tasks;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public interface IAssetService
    {
        Task<Asset> GetAsset(string address, string ticker);
        Task<Asset[]> GetAssets(int limit, bool orderAsc, string startingAfter,
            string endingBefore);
    }
}
