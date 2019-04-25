namespace Lykke.Job.Bil2Indexer.Domain
{
    public static class CorrelationIdType
    {
        public static string Parse(string correlationId)
        {
            return correlationId.Substring(0, 3);
        }
    }
}