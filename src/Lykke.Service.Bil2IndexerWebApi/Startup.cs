using System;
using JetBrains.Annotations;
using Lykke.Logs.Loggers.LykkeSlack;
using Lykke.Sdk;
using Lykke.Service.Bil2IndexerWebApi.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.Bil2IndexerWebApi
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
                options.DisableFluentValidation();
                options.DisableValidationFilter();

                options.SwaggerOptions = _swaggerOptions;

                options.Logs = logs =>
                {
                    logs.AzureTableName = "Bil2IndexerWebApiLog";
                    logs.AzureTableConnectionStringResolver = settings => settings.Bil2WebApiService.Db.LogsConnString;

                    logs.Extended = extendedLogs =>
                    {
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
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }



            app.UseLykkeConfiguration(options =>
            {
                options.DisableUnhandledExceptionLoggingMiddleware();
                options.DisableValidationExceptionMiddleware();
                // TODO: Add option to specify empty RoutePrefix for swagger to the Lykke.Sdk
                options.SwaggerOptions = _swaggerOptions;
            });
        }
    }
}
