using System.Threading.Tasks;

namespace Lykke.Job.Bil2Indexer.Infrastructure
{
    public static class TaskExecution
    {
        public static async Task<(TResult1, TResult2)> WhenAll<TResult1, TResult2>(Task<TResult1> task1, Task<TResult2> task2)
        {
            await Task.WhenAll(task1, task2);

            return (task1.Result, task2.Result);
        }

        public static async Task<(TResult1, TResult2, TResult3)> WhenAll<TResult1, TResult2, TResult3>(Task<TResult1> task1, Task<TResult2> task2, Task<TResult3> task3)
        {
            await Task.WhenAll(task1, task2, task3);

            return (task1.Result, task2.Result, task3.Result);
        }

        public static async Task<(TResult1, TResult2, TResult3, TResult4)> WhenAll<TResult1, TResult2, TResult3, TResult4>(Task<TResult1> task1, Task<TResult2> task2, Task<TResult3> task3, Task<TResult4> task4)
        {
            await Task.WhenAll(task1, task2, task3, task4);

            return (task1.Result, task2.Result, task3.Result, task4.Result);
        }
    }
}
