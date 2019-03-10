using System;
using System.Diagnostics;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    public static class Waiting
    {
        public static TimeSpan Timeout { get; }

        static Waiting()
        {
            Timeout = !Debugger.IsAttached ? TimeSpan.FromSeconds(1) : System.Threading.Timeout.InfiniteTimeSpan;
        }
    }
}
