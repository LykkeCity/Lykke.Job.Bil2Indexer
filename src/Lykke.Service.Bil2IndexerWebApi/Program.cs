using System.Threading.Tasks;
using Lykke.Sdk;

namespace Lykke.Service.Bil2IndexerWebApi
{
    internal sealed class Program
    {
        public static async Task Main(string[] args)
        {
#if DEBUG
            await LykkeStarter.Start<Startup>(true, 5001);
#else
            await LykkeStarter.Start<Startup>(false);
#endif
        }
    }
}
