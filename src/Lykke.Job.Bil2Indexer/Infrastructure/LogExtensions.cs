using Common.Log;
using Lykke.Bil2.RabbitMq.Subscription;
using Lykke.Common.Log;

namespace Lykke.Job.Bil2Indexer.Infrastructure
{
    public static class LogExtensions
    {
        public static void LogLegacyMessage<TMessage>(this ILog log, TMessage message, MessageHeaders headers)
        {
            log.Info(process: typeof(TMessage).Name, "Message treated as legacy and ignored", new
            {
                Headers = headers, 
                Message = message
            });
        }
    }
}
