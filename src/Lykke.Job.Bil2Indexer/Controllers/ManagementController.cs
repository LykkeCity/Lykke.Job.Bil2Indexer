using System.Threading.Tasks;
using Lykke.Bil2.Client.BlocksReader.Services;
using Lykke.Bil2.Contract.BlocksReader.Commands;
using Lykke.Job.Bil2Indexer.Models.Management;
using Lykke.Job.Bil2Indexer.Services;
using Lykke.Job.Bil2Indexer.Workflow.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Job.Bil2Indexer.Controllers
{
    [Route("api/management")]
    public class ManagementController : ControllerBase
    {
        private readonly IMessageSendersFactory _messageSendersFactory;
        private readonly IBlocksReaderApiFactory _blocksReaderApiFactory;

        public ManagementController(
            IMessageSendersFactory messageSendersFactory,
            IBlocksReaderApiFactory blocksReaderApiFactory)
        {
            _messageSendersFactory = messageSendersFactory;
            _blocksReaderApiFactory = blocksReaderApiFactory;
        }

        [HttpPost("execute-transfer-coins-block")]
        public IActionResult ExecuteTransferCoinsBlock(ExecuteTransferCoinsBlockRequest request)
        {
            var commandsSender = _messageSendersFactory.CreateCommandsSender();

            commandsSender.Publish
            (
                new ExecuteTransferCoinsBlockCommand
                {
                    BlockchainType = request.BlockchainType,
                    BlockId = request.BlockId
                },
                request.CorrelationId
            );

            return Ok();
        }

        [HttpPost("extend-chain-head")]
        public IActionResult ExtendChainHead(ExtendChainHeadRequest request)
        {
            var commandsSender = _messageSendersFactory.CreateCommandsSender();

            commandsSender.Publish
            (
                new ExtendChainHeadCommand
                {
                    BlockchainType = request.BlockchainType,
                    ToBlockNumber = request.ToBlockNumber,
                    ToBlockId = request.ToBlockId
                },
                request.CorrelationId
            );

            return Ok();
        }

        [HttpPost("read-block")]
        public async Task<IActionResult> HandleNotFoundBlock(ReadBlockRequest request)
        {
            var blocksReaderApi = _blocksReaderApiFactory.Create(request.BlockchainType);

            await blocksReaderApi.SendAsync(new ReadBlockCommand(request.BlockNumber), request.CorrelationId);

            return Ok();
        }
    }
}
