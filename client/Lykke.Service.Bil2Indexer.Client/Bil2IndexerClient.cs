using Lykke.HttpClientGenerator;

namespace Lykke.Service.Bil2Indexer.Client
{
    /// <summary>
    /// Bil2Indexer API aggregating interface.
    /// </summary>
    public class Bil2IndexerClient : IBil2IndexerClient
    {
        // Note: Add similar Api properties for each new service controller

        /// <summary>Inerface to Bil2Indexer Api.</summary>
        public IBil2IndexerApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public Bil2IndexerClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IBil2IndexerApi>();
        }
    }
}
