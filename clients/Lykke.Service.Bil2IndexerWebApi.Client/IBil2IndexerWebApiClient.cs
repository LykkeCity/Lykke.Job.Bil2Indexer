using JetBrains.Annotations;

namespace Lykke.Service.Bil2IndexerWebApi.Client
{
    /// <summary>
    /// Bil2Indexer WEB API client.
    /// </summary>
    [PublicAPI]
    public interface IBil2IndexerWebApiClient
    {
        /// <summary>Application WEB API interface</summary>
        IBil2IndexerWebApi Api { get; }
    }
}
