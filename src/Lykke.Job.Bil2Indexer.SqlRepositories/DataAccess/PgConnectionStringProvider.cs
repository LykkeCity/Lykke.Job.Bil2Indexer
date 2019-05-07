using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Job.Bil2Indexer.Domain.Repositories;
using Microsoft.EntityFrameworkCore.Internal;

namespace Lykke.Job.Bil2Indexer.SqlRepositories.DataAccess
{
    public class PgConnectionStringProvider: IPgConnectionStringProvider
    {
        private readonly IReadOnlyDictionary<string, string> _btypeConnStringStorage;

        public PgConnectionStringProvider(IReadOnlyDictionary<string, string> btypeConnStringStorage)
        {
            if (btypeConnStringStorage.Values.Distinct().Count() > 1)
            {
                throw new ArgumentException("Separate databases should be used");
            }

            _btypeConnStringStorage = btypeConnStringStorage;
        }

        public string GetConnectionString(string blockchainType)
        {
            return _btypeConnStringStorage[blockchainType];
        }
    }
}
