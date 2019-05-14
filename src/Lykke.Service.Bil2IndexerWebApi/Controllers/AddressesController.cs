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
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
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
                var balances = await _addressService.GetBalancesByBlockId
                (
                    blockchainType,
                    address,
                    blockId,
                    limit,
                    order == PaginationOrder.Asc,
                    startingAfter,
                    endingBefore
                );

                return AddressBalanceModelMapper.Map(balances);
            }

            if (blockNumber != null)
            {
                var balances = await _addressService.GetBalancesByBlockNumber
                (
                    blockchainType,
                    address,
                    blockNumber.Value,
                    limit,
                    order == PaginationOrder.Asc,
                    startingAfter,
                    endingBefore
                );

                return AddressBalanceModelMapper.Map(balances);
            }

            if (datetime != null)
            {
                var balances = await _addressService.GetBalancesOnDate
                (
                    blockchainType,
                    address,
                    datetime.Value.UtcDateTime,
                    limit,
                    order == PaginationOrder.Asc,
                    startingAfter,
                    endingBefore
                );

                return AddressBalanceModelMapper.Map(balances);
            }

            {
                var balances = await _addressService.GetBalances
                (
                    blockchainType,
                    address,
                    limit,
                    order == PaginationOrder.Asc,
                    startingAfter,
                    endingBefore
                );

                return AddressBalanceModelMapper.Map(balances);
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
            var unspentOutputs = await _addressService.GetUnspentOutputs
            (
                blockchainType,
                addresses,
                limit,
                order == PaginationOrder.Asc,
                startingAfter,
                endingBefore
            );

            return AddressUnspentOutputModelMapper.Map(unspentOutputs);
        }
    }
}
