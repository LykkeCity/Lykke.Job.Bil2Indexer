using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain.Services.Infrastructure;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace Lykke.Job.Bil2Indexer.DomainServices.Infrastructure
{
    public class AppInsightTelemetryProvider : IAppInsightTelemetryProvider
    {
        private static readonly TelemetryClient TelemetryClient= new TelemetryClient();

        public async Task ExecuteMethodWithTelemetryAsync(string operationName, string operationId, Func<Task> awaitableFunc)
        {
            var operation = TelemetryClient.StartOperation<RequestTelemetry>(new RequestTelemetry()
            {
                Name = operationName,
                Id = operationId
            });

            try
            {
                await awaitableFunc();
            }
            catch (Exception e)
            {
                operation.Telemetry.Success = false;
                TelemetryClient.TrackException(e);

                throw;
            }
            finally
            {
                TelemetryClient.StopOperation(operation);
            }
        }

        public async Task<T> ExecuteMethodWithTelemetryAndReturnAsync<T>(string operationName, string operationId, Func<Task<T>> awaitableFunc)
        {
            var operation = TelemetryClient.StartOperation<RequestTelemetry>(new RequestTelemetry()
            {
                Name = operationName,
                Id = operationId
            });

            try
            {
                return await awaitableFunc();
            }
            catch (Exception e)
            {
                operation.Telemetry.Success = false;
                TelemetryClient.TrackException(e);

                throw;
            }
            finally
            {
                TelemetryClient.StopOperation(operation);
            }
        }

        public string FormatOperationName<TDecorated>(TDecorated repositoryName, [CallerMemberName]string methodName = null)
        {
            return $"Repository: {repositoryName.ToString()}:{methodName}";
        }
    }
}
