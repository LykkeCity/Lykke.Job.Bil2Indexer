using JetBrains.Annotations;
using Lykke.Job.Bil2Indexer.Modules;
using Lykke.Job.Bil2Indexer.Settings;
using Lykke.SettingsReader;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.Modules
{
    [UsedImplicitly]
    public class RepositoriesToolModule : RepositoriesModule
    {
        public RepositoriesToolModule(IReloadingManager<AppSettings> settings) : base(settings)
        {
        }
    }
}
