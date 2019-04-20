﻿using System.Threading.Tasks;
using Common.Log;
using Lykke.Bil2.RabbitMq;
using Lykke.Common.Log;
using Lykke.Sdk;
using Grpc.Core;

namespace Lykke.Service.Bil2IndexerGrpcApi.Services
{
    public class StartupManager : IStartupManager
    {
        private readonly Server _grpcServer;
        private readonly RabbitMqConfigurator _rabbitMqConfigurator;
        private readonly IRabbitMqEndpoint _rabbitMqEndpoint;
        private readonly ILog _log;

        public StartupManager(
            ILogFactory logFactory,
            RabbitMqConfigurator rabbitMqConfigurator,
            IRabbitMqEndpoint rabbitMqEndpoint,
            Server grpcServer)
        {
            _rabbitMqConfigurator = rabbitMqConfigurator;
            _rabbitMqEndpoint = rabbitMqEndpoint;

            _log = logFactory.CreateLog(this);
            _grpcServer = grpcServer;
        }

        public Task StartAsync()
        {
            _log.Info("Starting GRPC endpoint...");
            _grpcServer.Start();

            _log.Info("Initializing RabbitMQ endpoint...");
            
            _rabbitMqEndpoint.Initialize();

            _log.Info("Initializing RabbitMQ messaging configuration...");

            _rabbitMqConfigurator.Configure();

            _log.Info("Starting RabbitMQ endpoint...");

            _rabbitMqEndpoint.StartListening();

            return Task.CompletedTask;
        }
    }
}
