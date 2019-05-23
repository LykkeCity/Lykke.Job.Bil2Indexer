using System.Collections.Generic;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public static class SupportedBlockchains
    {
        //TODO move to service level
        public static IEnumerable<string> List => new[] {"bitcoin", "ripple"};
    }
}
