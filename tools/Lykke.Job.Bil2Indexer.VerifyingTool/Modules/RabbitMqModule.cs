using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.Modules;
using Lykke.Job.Bil2Indexer.Settings;
using Lykke.SettingsReader;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.Modules
{
    [UsedImplicitly]
    public class RabbitMqToolModule : RabbitMqModule
    {
        public RabbitMqToolModule(IReloadingManager<AppSettings> settings) : 
            base(settings)
        {
        }
    }
}
