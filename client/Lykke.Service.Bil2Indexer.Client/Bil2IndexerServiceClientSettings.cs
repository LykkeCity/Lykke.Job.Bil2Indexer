using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2Indexer.Client 
{
    /// <summary>
    /// Bil2Indexer client settings.
    /// </summary>
    public class Bil2IndexerServiceClientSettings 
    {
        /// <summary>Service url.</summary>
        [HttpCheck("api/isalive")]
        public string ServiceUrl {get; set;}
    }
}
