using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Sdk;
using Grpc.Core;

namespace Lykke.Service.Bil2IndexerGrpcApi.Services
{
    public class ShutdownManager : IShutdownManager
    {
        private readonly ILog _log;
        private readonly IEnumerable<IStopable> _items;
        private readonly Server _grpcServer;

        public ShutdownManager(
            ILogFactory logFactory, 
            IEnumerable<IStopable> items,
            Server grpcServer)
        {
            _log = logFactory.CreateLog(this);
            _items = items;
            _grpcServer = grpcServer;
        }

        public async Task StopAsync()
        {
            await _grpcServer.ShutdownAsync();

            // TODO: Implement your shutdown logic here. Good idea is to log every step
            foreach (var item in _items)
            {
                try
                {
                    item.Stop();
                }
                catch (Exception ex)
                {
                    _log.Warning($"Unable to stop {item.GetType().Name}", ex);
                }
            }
            
            await Task.CompletedTask;
        }
    }
}
