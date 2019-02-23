using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;

namespace Lykke.Job.Bil2Indexer.Tests.Mocks
{
    internal class BlocksReaderApiMock : IBlocksReaderApi
    {
        private readonly InMemoryReadBlockCommandsQueue _commandsQueue;
        
        public BlocksReaderApiMock(InMemoryReadBlockCommandsQueue commandsQueue)
        {
            _commandsQueue = commandsQueue;
        }

        public Task SendAsync(ReadBlockCommand command)
        {
            _commandsQueue.Send(command);

            return Task.CompletedTask;
        }
    }
}
