using System;
using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Domain
{
    public class BlockExpectation
    {
        public string Version { get; }
        public long Number { get; }

        public BlockExpectation(long number)
        {
            Number = number;
        }

        public BlockExpectation(string version, long number)
        {
            Version = version;
            Number = number;
        }

        [Pure]
        public BlockExpectation Skip(long blocksNumber)
        {
            if (blocksNumber < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(blocksNumber), blocksNumber, "Should be positive number");
            }

            return new BlockExpectation(Version, Number + blocksNumber);
        }

        [Pure]
        public BlockExpectation Previous()
        {
            return new BlockExpectation(Version, Number - 1);
        }

        public override string ToString()
        {
            return $"{Number} : {Version}";
        }
    }
}
