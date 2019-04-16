using JetBrains.Annotations;
using Lykke.Sdk;
using Lykke.Service.Bil2Indexer.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using Lykke.Logs.Loggers.LykkeSlack;

namespace Lykke.Service.Bil2Indexer
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly LykkeSwaggerOptions _swaggerOptions = new LykkeSwaggerOptions
        {
            ApiTitle = "Bil2Indexer API",
            ApiVersion = "v1"
        };

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return services.BuildServiceProvider<AppSettings>(options =>
            {
                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "Bil2IndexerServiceLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.Bil2IndexerService.Db.LogsConnString;
                    
                    logs.Extended = extendedLogs =>
                    {
                        //extendedLogs.AddFilter((provider, component, level) => provider == "Lykke.Logs.Loggers.LykkeConsole.LykkeConsoleLoggerProvider");
                       
                        extendedLogs.AddAdditionalSlackChannel("CommonBlockChainIntegration", channelOptions =>
                        {
                            channelOptions.MinLogLevel = Microsoft.Extensions.Logging.LogLevel.Information;
                            channelOptions.SpamGuard.DisableGuarding();
                            channelOptions.IncludeHealthNotifications();
                        });

                        extendedLogs.AddAdditionalSlackChannel("CommonBlockChainIntegrationImportantMessages",channelOptions =>
                        {
                            channelOptions.MinLogLevel = Microsoft.Extensions.Logging.LogLevel.Warning;
                            channelOptions.SpamGuard.DisableGuarding();
                            channelOptions.IncludeHealthNotifications();
                        });
                    };
                };
            });
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
