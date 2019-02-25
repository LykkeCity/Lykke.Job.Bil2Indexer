using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.DomainServices
{
    internal static class TaskExecution
    {
        public static async Task<(TResult1, TResult2)> WhenAll<TResult1, TResult2>(Task<TResult1> task1, Task<TResult2> task2)
        {
            await Task.WhenAll(task1, task2);

            return (task1.Result, task2.Result);
        }
    }
}
