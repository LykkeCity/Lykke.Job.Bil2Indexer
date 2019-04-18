using System.Threading.Tasks;
using Lykke.Sdk;

namespace Lykke.Service.Bil2IndexerGrpcApi
{
    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
#if DEBUG
            await LykkeStarter.Start<Startup>(true, 5002);
#else
            await LykkeStarter.Start<Startup>(false);
#endif
        }
    }
}
