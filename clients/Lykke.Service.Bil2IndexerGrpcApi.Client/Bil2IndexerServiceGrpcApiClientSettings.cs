using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2IndexerGrpcApi.Client 
{
    /// <summary>
    /// Bil2Indexer client settings.
    /// </summary>
    [PublicAPI]
    public class Bil2IndexerServiceGrpcApiClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
