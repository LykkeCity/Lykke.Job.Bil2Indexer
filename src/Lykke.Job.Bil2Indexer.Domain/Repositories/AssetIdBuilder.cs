using System;
using System.Linq;
using Lykke.Bil2.SharedDomain;

namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public static class AssetIdBuilder
    {
        private const string Delimenator = "_";
        public static string BuildId(this Asset asset)
        {
            if (asset.Address?.Value == null)
            {
                return asset.Id;
            }

            return $"{asset.Id}{Delimenator}{asset.Address}";
        }

        public static Asset BuildDomainOrDefault(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return null;
            }

            if (!source.Contains(Delimenator))
            {
                return new Asset(source);
            }

            var segments = source.Split(Delimenator);

            return new Asset(segments.Take(1).First(), segments.Skip(1).SingleOrDefault());
        }
    }
}
