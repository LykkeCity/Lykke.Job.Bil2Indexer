using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Settings.BlockchainIntegrations;

namespace Lykke.Job.Bil2Indexer.Services
{
    public class IntegrationSettingsProvider
    {
        private readonly IReadOnlyDictionary<string, BlockchainIntegrationSettings> _integrationsSettings;

        public IntegrationSettingsProvider(IReadOnlyCollection<BlockchainIntegrationSettings> integrationsSettings)
        {
            _integrationsSettings = integrationsSettings.ToDictionary(x => x.Type, x => x);
        }

        public BlockchainIntegrationSettings Get(string integrationType)
        {
            if (!_integrationsSettings.TryGetValue(integrationType, out var settings))
            {
                throw new InvalidOperationException($"Settings for the integration {integrationType} is not found");
            }

            return settings;
        }
    }
}
