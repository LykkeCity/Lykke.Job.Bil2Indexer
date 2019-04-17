using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2IndexerWebApi.Client 
{
    /// <summary>
    /// Bil2Indexer WEB API client settings.
    /// </summary>
    [PublicAPI]
    public class Bil2IndexerServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
