using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    internal class BlocksReaderApiMock : IBlocksReaderApi
    {
        private readonly ChainsEvaluator _chainsEvaluator;
        
        public BlocksReaderApiMock(ChainsEvaluator chainsEvaluator)
        {
            _chainsEvaluator = chainsEvaluator;
        }

        public Task SendAsync(ReadBlockCommand command)
        {
            _chainsEvaluator.EvaluateBlock(command.BlockNumber);

            return Task.CompletedTask;
        }
    }
}
