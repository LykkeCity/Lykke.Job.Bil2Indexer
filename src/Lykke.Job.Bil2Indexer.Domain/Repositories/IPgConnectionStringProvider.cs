namespace Lykke.Job.Bil2Indexer.Domain.Repositories
{
    public interface IPgConnectionStringProvider
    {
        string GetConnectionString(string blockchainType);
    }
}
