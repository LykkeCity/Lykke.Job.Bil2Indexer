using JetBrains.Annotations;

namespace Lykke.Service.Bil2IndexerGrpcApi.Client
{
    /// <summary>
    /// Bil2Indexer GRPC API client.
    /// </summary>
    [PublicAPI]
    public interface IBil2IndexerGrpcClient
    {
        /// <summary>Application GRPC Api interface</summary>
        IBil2IndexerGrpcApi GrpcApi { get; }
    }
}
