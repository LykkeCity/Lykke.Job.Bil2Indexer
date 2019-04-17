using Lykke.Common;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Lykke.Job.Bil2Indexer.Settings;
using Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters;
using Lykke.Job.Bil2Indexer.VerifyingTool.Reporting;
using Lykke.Job.Bil2Indexer.VerifyingTool.StartUpFolder;
using Lykke.Logs.Loggers.LykkeConsole;
using Lykke.Sdk;
using Lykke.SettingsReader;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using BlockHeader = Lykke.Job.Bil2Indexer.Domain.BlockHeader;

namespace Lykke.Job.Bil2Indexer.VerifyingTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            BigInteger fromBlock = BigInteger.Parse(args[0]);
            BigInteger toBlock = BigInteger.Parse(args[1]);
            string blockchainType = args[2];
            string dataBaseConnectionString = args[3];
            bool isIndexingInMemoryEnabled = bool.Parse(args[4]);
            var normalArgsCount = 5;
            var restLength = args.Length - normalArgsCount;
            string[] restArgs = new string[restLength];
            Array.Copy(args, normalArgsCount, restArgs, 0, restLength);

            var logFactory = Logs.EmptyLogFactory.Instance;
            logFactory.AddConsole();
            Task taskWithIndexer;
            if (isIndexingInMemoryEnabled)
            {
                taskWithIndexer = Task.Run(() =>
                {
                    return LykkeStarter.Start<StartupInMemory>(true, 5001);
                });
            }
            await Task.Delay(45000);
            //TODO: Inint StartupInMemory.ServiceProvider with repo services.
            IBlockHeadersRepository blockHeadersRepository =
                StartupInMemory.ServiceProvider.GetRequiredService<IBlockHeadersRepository>();
            ITransactionsRepository transactionsRepository =
                StartupInMemory.ServiceProvider.GetRequiredService<ITransactionsRepository>();

            var appSettings = ReloadingManagerWithConfiguration();
            var blockchainIntegrationSettings = appSettings
                .CurrentValue
                .BlockchainIntegrations
                .FirstOrDefault(x => x.Type == blockchainType);

            if (isIndexingInMemoryEnabled)
            {
                BlockHeader header = null;

                do
                {
                    header = await blockHeadersRepository.GetOrDefaultAsync(blockchainType, (long)toBlock -1);

                    if (header != null)
                    {
                        break;
                    }
                    else
                    {
                        await Task.Delay(10000);
                    }

                } while (true);
            }

            var adapter = InitAdapter(blockchainType, restArgs);
            var report = new Report(blockHeadersRepository,
                blockchainType,
                adapter,
                blockchainIntegrationSettings.Capabilities.TransferModel,
                transactionsRepository);

            await report.CreateReportAsync(fromBlock, toBlock);
        }

        private static IReloadingManagerWithConfiguration<AppSettings> ReloadingManagerWithConfiguration()
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();

            var settings = configurationRoot.LoadSettings<AppSettings>(options =>
            {
                options.SetConnString(x => x.SlackNotifications?.AzureQueue.ConnectionString);
                options.SetQueueName(x => x.SlackNotifications?.AzureQueue.QueueName);
                options.SenderName = $"{AppEnvironment.Name} {AppEnvironment.Version}";
            });
            
            return settings;
        }

        private static IBlockchainVerifierAdapter InitAdapter(string blockchainType, string[] args)
        {
            BlockchainVerifierAdapterFactory factory = new BlockchainVerifierAdapterFactory();
            return factory.GetAdapter(blockchainType, args);
        }
    }
}
