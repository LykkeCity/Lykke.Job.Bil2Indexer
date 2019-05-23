﻿using Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.Bil2IndexerWebApi.Settings.ApiSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnString { get; set; }
    }
}
