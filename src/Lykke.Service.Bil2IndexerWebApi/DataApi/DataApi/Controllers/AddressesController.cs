using System;
using System.Threading.Tasks;
using DataApi.Core.Domain;
using DataApi.Factories;
using DataApi.Models;
using DataApi.Models.Common;
using DataApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataApi.Controllers
{
    [Route("api/[controller]")]
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

        [HttpGet("/addresses/{id}/balances", Name = nameof(GetAddressBalances))]
        public async Task<ActionResult<Paginated<BalanceModel[]>>> GetAddressBalances([FromRoute] string id,
            [FromQuery] string blockId, [FromQuery] int? blockNumber, DateTime? date,
            PaginationOrder order, string startingAfter, string endingBefore, int limit = 25)
        {
            Balance[] balances = null;

            if (id != null)
            {
                balances = await _addressService.GetBalancesByAddress(id, limit, order == PaginationOrder.Asc,
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
                balances = await _addressService.GetBalancesOnDate(date.Value, limit,
                    order == PaginationOrder.Asc,
                    startingAfter, endingBefore);
            }

            var model = _addressModelFactory.PrepareBalancesPaginated(balances);

            return model;
        }

        [HttpGet("/addresses/{id}/unspentOutputs", Name = nameof(GetAddressUnspentOutputs))]
        public async Task<ActionResult<Paginated<UnspentOutputModel[]>>> GetAddressUnspentOutputs([FromRoute] string id,
            PaginationOrder order, string startingAfter, string endingBefore, int limit = 25)
        {
            var unspentOutputs = await _addressService.GetUnspentOutputs(id, limit, order == PaginationOrder.Asc,
                startingAfter, endingBefore);

            var model = _addressModelFactory.PrepareUnspentOutputsPaginated(unspentOutputs);

            return model;
        }
    }
}