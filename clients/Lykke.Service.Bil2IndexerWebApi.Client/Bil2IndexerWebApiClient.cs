using Lykke.HttpClientGenerator;

namespace Lykke.Service.Bil2IndexerWebApi.Client
{
    /// <summary>
    /// Bil2Indexer WEB API client.
    /// </summary>
    public class Bil2IndexerWebApiClient : IBil2IndexerWebApiClient
    {
        /// <summary>Bil2Indexer WEB Api.</summary>
        public IBil2IndexerWebApi Api { get; private set; }

        /// <summary>C-tor</summary>
        public Bil2IndexerWebApiClient(IHttpClientGenerator httpClientGenerator)
        {
            Api = httpClientGenerator.Generate<IBil2IndexerWebApi>();
        }
    }
}
