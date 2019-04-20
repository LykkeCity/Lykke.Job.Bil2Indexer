using System;
using JetBrains.Annotations;

namespace Lykke.Job.Bil2Indexer.Settings.JobSettings
{
    [UsedImplicitly]
    public class BlocksAssemblingSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan RetryTimeout { get; set; }
    }
}