using JetBrains.Annotations;

namespace Lykke.Service.Bil2IndexerGrpcApi.Client
{
    /// <summary>
    /// Bil2Indexer GRPC API client.
    /// </summary>
    [PublicAPI]
    public class Bil2IndexerGrpcClient : IBil2IndexerGrpcClient
    {
        /// <summary>Bil2Indexer GRPC GrpcApi.</summary>
        public IBil2IndexerGrpcApi GrpcApi { get; private set; }

        //// <summary>C-tor</summary>
        //public Bil2IndexerGrpcClient(IHttpClientGenerator httpClientGenerator)
        //{
        //    GrpcApi = httpClientGenerator.Generate<IBil2IndexerGrpcApi>();
        //}
    }
}
