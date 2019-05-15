using System;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    [Route("api/blockchains/{blockchainType}/addresses")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressQueryFacade _addressQueryFacade;

        public AddressesController(IAddressQueryFacade addressQueryFacade)
        {
            _addressQueryFacade = addressQueryFacade;
        }

        [HttpGet("/{address}/balances", Name = nameof(GetAddressBalances))]
        public async Task<ActionResult<Paginated<AddressBalanceModel>>> GetAddressBalances(
            [FromRoute] string blockchainType,
            [FromRoute] string address,
            [FromQuery] string blockId, 
            [FromQuery] int? blockNumber, 
            [FromQuery] DateTimeOffset? datetime,
            [FromQuery] PaginationOrder order, 
            [FromQuery] string startingAfter, 
            [FromQuery] string endingBefore, 
            [FromQuery] int limit = 25)
        {
            // TODO: Validate parameters

            if (blockId != null)
            {
                var balances = await _addressQueryFacade.GetBalancesByBlockId
                (
                    blockchainType,
                    address,
                    blockId,
                    limit,
                    order == PaginationOrder.Asc,
                    startingAfter,
                    endingBefore
                );

                return AddressBalanceModelMapper.ToViewModel(balances);
            }

            if (blockNumber != null)
            {
                var balances = await _addressQueryFacade.GetBalancesByBlockNumber
                (
                    blockchainType,
                    address,
                    blockNumber.Value,
                    limit,
                    order == PaginationOrder.Asc,
                    startingAfter,
                    endingBefore
                );

                return AddressBalanceModelMapper.ToViewModel(balances);
            }

            if (datetime != null)
            {
                var balances = await _addressQueryFacade.GetBalancesOnDate
                (
                    blockchainType,
                    address,
                    datetime.Value.UtcDateTime,
                    limit,
                    order == PaginationOrder.Asc,
                    startingAfter,
                    endingBefore
                );

                return AddressBalanceModelMapper.ToViewModel(balances);
            }

            {
                return await _addressQueryFacade.GetBalances
                (
                    blockchainType,
                    address,
                    limit,
                    order == PaginationOrder.Asc,
                    startingAfter,
                    endingBefore
                );
            }
        }

        [HttpGet("/{address}/unspent-outputs", Name = nameof(GetAddressUnspentOutputs))]
        public async Task<ActionResult<Paginated<AddressUnspentOutputModel>>> GetAddressUnspentOutputs(
            [FromRoute] string blockchainType,
            [FromRoute] string addresses,
            [FromQuery] PaginationOrder order, 
            [FromQuery] string startingAfter,
            [FromQuery] string endingBefore, 
            [FromQuery] int limit = 25)
        {
            var unspentOutputs = await _addressQueryFacade.GetUnspentOutputs
            (
                blockchainType,
                addresses,
                limit,
                order == PaginationOrder.Asc,
                startingAfter,
                endingBefore
            );

            return AddressUnspentOutputModelMapper.ToViewModel(unspentOutputs);
        }
    }
}
