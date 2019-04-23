using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Decorators
{
    public class AppInsightTelemetryProvider : IAppInsightTelemetryProvider
    {
        private static readonly TelemetryClient TelemetryClient= new TelemetryClient();

        public AppInsightTelemetryProvider()
        {
        }

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

        public string FormatOperationName(string repositoryName, string methodName)
        {
            return $"Repository: {repositoryName}:{methodName}";
        }
    }
}
