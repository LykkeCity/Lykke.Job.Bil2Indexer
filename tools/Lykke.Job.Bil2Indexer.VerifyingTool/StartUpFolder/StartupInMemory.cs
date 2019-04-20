using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.Settings;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.Sdk;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.StartUpFolder
{
    [UsedImplicitly]
    public class StartupInMemory
    {
        public static IServiceProvider ServiceProvider { get; set; }

        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "Bil2IndexerJob API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            ServiceProvider = services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "Bil2IndexerJobLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.Bil2IndexerJob.Db.LogsConnString;

                    logs.Extended = extendedLogs =>
                    {
                        //extendedLogs.AddFilter((provider, component, level) => provider == "Lykke.Logs.Loggers.LykkeConsole.LykkeConsoleLoggerProvider");

                        extendedLogs.AddAdditionalSlackChannel("CommonBlockChainIntegration", channelOptions =>
                        {
                            channelOptions.MinLogLevel = Microsoft.Extensions.Logging.LogLevel.Information;
                            channelOptions.SpamGuard.DisableGuarding();
                            channelOptions.IncludeHealthNotifications();
                        });

                        extendedLogs.AddAdditionalSlackChannel("CommonBlockChainIntegrationImportantMessages", channelOptions =>
                        {
                            channelOptions.MinLogLevel = Microsoft.Extensions.Logging.LogLevel.Warning;
                            channelOptions.SpamGuard.DisableGuarding();
                            channelOptions.IncludeHealthNotifications();
                        });
                    };
                };
            });

            return ServiceProvider;
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app)
        {
            app.UseLykkeConfiguration(options =>
            {
                options.SwaggerOptions = _swaggerOptions;
            });
        }
    }
}
