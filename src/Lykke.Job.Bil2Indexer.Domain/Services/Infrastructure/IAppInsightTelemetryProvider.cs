using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Domain.Services.Infrastructure
{
    public interface IAppInsightTelemetryProvider
    {
        Task ExecuteMethodWithTelemetryAsync(string operationName, string operationId, Func<Task> awaitableFunc);

        Task<T> ExecuteMethodWithTelemetryAndReturnAsync<T>(string operationName, string operationId, Func<Task<T>> awaitableFunc);

        string FormatOperationName<TDecorated>(TDecorated repositoryName, [CallerMemberName] string methodName = null);
    }
}
