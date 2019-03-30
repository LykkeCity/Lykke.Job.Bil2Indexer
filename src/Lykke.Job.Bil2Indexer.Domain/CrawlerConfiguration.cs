using System;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class CrawlerConfiguration : IEquatable<CrawlerConfiguration>
    {
        public long StartBlock { get; }
        public long? StopBlock { get; }

        public CrawlerConfiguration(long startBlock, long? stopBlock)
        {
            StartBlock = startBlock;
            StopBlock = stopBlock;
        }

        public static CrawlerConfiguration Parse(string configurationString)
        {
            if (configurationString == null)
            {
                throw new ArgumentNullException(nameof(configurationString));
            }
           
            var parts = configurationString.Split('-');

            if (parts.Length != 2)
            {
                throw new ArgumentException(
                    $"Crawler configuration should be in the format '<startBlock>-<stopBlock>' or '<startBlock>-*'. Actual configurationString: {configurationString}",
                    configurationString);
            }

            if (!long.TryParse(parts[0], out var startBlock))
            {
                throw new ArgumentException(
                    $"Can't pars start block as long. Start block: {parts[0]}. Actual configurationString: {configurationString}", 
                    configurationString);
            }

            long? stopBlock;

            if (parts[1] == "*")
            {
                stopBlock = null;
            }
            else
            {
                if (!long.TryParse(parts[1], out var stopBlockValue))
                {
                    throw new ArgumentException(
                        $"Can't pars stop block as long and it's not '*'. Stop block: {parts[1]}. Actual configurationString: {configurationString}",
                        configurationString);
                }

                stopBlock = stopBlockValue;
            }

            return new CrawlerConfiguration(startBlock, stopBlock);
        }

        public bool CanProcess(long blockNumber)
        {
            return blockNumber >= StartBlock && (!StopBlock.HasValue || blockNumber < StopBlock.Value);
        }

        public override string ToString()
        {
            return StopBlock.HasValue ? $"{StartBlock}-{StopBlock}" : $"{StartBlock}-*";
        }
        
        public bool Equals(CrawlerConfiguration other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return StartBlock == other.StartBlock && StopBlock == other.StopBlock;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((CrawlerConfiguration) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (StartBlock.GetHashCode() * 397) ^ StopBlock.GetHashCode();
            }
        }
    }
}
