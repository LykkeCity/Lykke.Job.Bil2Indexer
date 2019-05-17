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
            PaginationRequest pagination)
        {
            // TODO: Validate parameters

            Paginated<AddressBalanceModel> result;
            if (blockId != null)
            {
                var balances = await _addressQueryFacade.GetBalancesByBlockId
                (
                    blockchainType,
                    address,
                    blockId,
                    pagination.Limit,
                    pagination.Order == PaginationOrder.Asc,
                    pagination.StartingAfter,
                    pagination.EndingBefore
                );

                result = balances.Paginate(pagination);

                return result;
            }

            if (blockNumber != null)
            {
                var balances = await _addressQueryFacade.GetBalancesByBlockNumber
                (
                    blockchainType,
                    address,
                    blockNumber.Value,
                    pagination.Limit,
                    pagination.Order == PaginationOrder.Asc,
                    pagination.StartingAfter,
                    pagination.EndingBefore
                );

                result = balances.Paginate(pagination);

                return result;
            }

            if (datetime != null)
            {
                var balances = await _addressQueryFacade.GetBalancesOnDate
                (
                    blockchainType,
                    address,
                    datetime.Value.UtcDateTime,
                    pagination.Limit,
                    pagination.Order == PaginationOrder.Asc,
                    pagination.StartingAfter,
                    pagination.EndingBefore
                );

                result = balances.Paginate(pagination);

                return result;
            }
            
            result = (await _addressQueryFacade.GetBalances
            (
                blockchainType,
                address,
                pagination.Limit,
                pagination.Order == PaginationOrder.Asc,
                pagination.StartingAfter,
                pagination.EndingBefore
            )).Paginate(pagination);

            return result;
        }

        [HttpGet("/{address}/unspent-outputs", Name = nameof(GetAddressUnspentOutputs))]
        public async Task<ActionResult<Paginated<AddressUnspentOutputModel>>> GetAddressUnspentOutputs(
            [FromRoute] string blockchainType,
            [FromRoute] string addresses,
            PaginationRequest pagination)
        {
            var result = await _addressQueryFacade.GetUnspentOutputs
            (
                blockchainType,
                addresses,
                pagination.Limit,
                pagination.Order == PaginationOrder.Asc,
                pagination.StartingAfter,
                pagination.EndingBefore
            );

            return result.Paginate(pagination);
        }
    }
}
