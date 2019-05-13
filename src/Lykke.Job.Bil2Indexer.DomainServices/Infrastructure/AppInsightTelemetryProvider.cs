﻿using System;
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
            var startedAt = DateTime.UtcNow;
            var success = true;

            try
            {
                await awaitableFunc();
            }
            catch (Exception e)
            {
                success = false;
                TelemetryClient.TrackException(e);

                throw;
            }
            finally
            {
                TelemetryClient.TrackDependency(new DependencyTelemetry
                {
                    Id = operationId,
                    Duration = DateTime.UtcNow - startedAt,
                    Name = operationName,
                    Success = success
                });
            }
        }

        public async Task<T> ExecuteMethodWithTelemetryAndReturnAsync<T>(string operationName, string operationId, Func<Task<T>> awaitableFunc)
        {
            var success = true;
            var startedAt = DateTime.UtcNow;

            try
            {
                return await awaitableFunc();
            }
            catch (Exception e)
            {
                success = false;
                TelemetryClient.TrackException(e);

                throw;
            }
            finally
            {
                TelemetryClient.TrackDependency(new DependencyTelemetry
                {
                    Id = operationId,
                    Duration = DateTime.UtcNow - startedAt,
                    Name = operationName,
                    Success = success
                });
            }
        }

        public string FormatOperationName<TDecorated>(TDecorated repositoryName, [CallerMemberName]string methodName = null)
        {
            return $"{typeof(TDecorated).Name}:{methodName}";
        }
    }
}
