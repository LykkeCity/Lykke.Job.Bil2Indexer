using System;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Factories;
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
        private readonly IAddressModelFactory _addressModelFactory;

        public AddressesController(IAddressService addressService, IAddressModelFactory addressModelFactory)
        {
            _addressService = addressService;
            _addressModelFactory = addressModelFactory;
        }

        [HttpGet("/{address}/balances", Name = nameof(GetAddressBalances))]
        public async Task<ActionResult<Paginated<AddressBalanceModel[]>>> GetAddressBalances(
            [FromRoute] string blockchainType,
            [FromRoute] string address,
            [FromQuery] string blockId, 
            [FromQuery] int? blockNumber, 
            [FromQuery] DateTimeOffset? datetime,
            PaginationOrder order, 
            string startingAfter, 
            string endingBefore, 
            int limit = 25)
        {
            Balance[] balances = null;

            if (address != null)
            {
                balances = await _addressService.GetBalancesByAddress(address, limit, order == PaginationOrder.Asc,
                    startingAfter, endingBefore);
            }

            if (blockId != null)
            {
                balances = await _addressService.GetBalancesByBlockId(blockId, limit, order == PaginationOrder.Asc,
                    startingAfter, endingBefore);
            }

            if (blockNumber != null)
            {
                balances = await _addressService.GetBalancesByBlockNumber(blockNumber.Value, limit,
                    order == PaginationOrder.Asc,
                    startingAfter, endingBefore);
            }

            if (date != null)
            {
                balances = await _addressService.GetBalancesOnDate((date.Value.UtcDateTime, limit,
                    order == PaginationOrder.Asc,
                    startingAfter, endingBefore);
            }

            var model = _addressModelFactory.PrepareBalancesPaginated(balances);

            return model;
        }

        [HttpGet("/{address}/unspent-outputs", Name = nameof(GetAddressUnspentOutputs))]
        public async Task<ActionResult<Paginated<AddressUnspentOutputModel[]>>> GetAddressUnspentOutputs(
            [FromRoute] string blockchainType,
            [FromRoute] string addresses,
            PaginationOrder order, 
            string startingAfter,
            string endingBefore, 
            int limit = 25)
        {
            var unspentOutputs = await _addressService.GetUnspentOutputs(addresses, limit, order == PaginationOrder.Asc,
                startingAfter, endingBefore);

            var model = _addressModelFactory.PrepareUnspentOutputsPaginated(unspentOutputs);

            return model;
        }
    }
}
