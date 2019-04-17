//using System;
//using Autofac;
//using JetBrains.Annotations;

//namespace Lykke.Service.Bil2IndexerGrpcApi.Client
//{
//    /// <summary>
//    /// Extension for GRPC API client registration
//    /// </summary>
//    [PublicAPI]
//    public static class AutofacExtension
//    {
//        /// <summary>
//        /// Registers <see cref="IBil2IndexerGrpcClient"/> in Autofac container using <see cref="Bil2IndexerServiceGrpcApiClientSettings"/>.
//        /// </summary>
//        /// <param name="builder">Autofac container builder.</param>
//        /// <param name="settings">Bil2Indexer GRPC API client settings.</param>
//        public static void RegisterBil2IndexerGrpcApiClient(
//            [NotNull] this ContainerBuilder builder,
//            [NotNull] Bil2IndexerServiceGrpcApiClientSettings settings)
//        {
//            if (builder == null)
//                throw new ArgumentNullException(nameof(builder));
//            if (settings == null)
//                throw new ArgumentNullException(nameof(settings));
//            if (string.IsNullOrWhiteSpace(settings.ServiceUrl))
//                throw new ArgumentException("Value cannot be null or whitespace.", nameof(Bil2IndexerServiceGrpcApiClientSettings.ServiceUrl));

//            //var clientBuilder = HttpClientGenerator.HttpClientGenerator.BuildForUrl(settings.ServiceUrl)
//            //    .WithAdditionalCallsWrapper(new ExceptionHandlerCallsWrapper());
            
//            //builder.RegisterInstance(new Bil2IndexerGrpcClient(clientBuilder.Create()))
//            //    .As<IBil2IndexerGrpcClient>()
//            //    .SingleInstance();
//        }
//    }
//}
