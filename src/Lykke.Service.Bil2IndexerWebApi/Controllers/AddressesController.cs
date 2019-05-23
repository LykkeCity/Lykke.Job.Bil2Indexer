using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.Bil2IndexerWebApi.Mappers;
using Lykke.Service.Bil2IndexerWebApi.Models;
using Lykke.Service.Bil2IndexerWebApi.Models.Common;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Service.Bil2IndexerWebApi.Controllers
{
    public class AddressesController : ControllerBase
    {
        private const string RoutePrefix = "api/blockchains/{blockchainType}/addresses";
        private readonly IAddressQueryFacade _addressQueryFacade;

        public AddressesController(IAddressQueryFacade addressQueryFacade)
        {
            _addressQueryFacade = addressQueryFacade;
        }
        
        [HttpGet(RoutePrefix + "/{address}/balances", Name = nameof(GetAddressBalances))]
        public async Task<ActionResult<IReadOnlyCollection<AddressBalanceResponce>>> GetAddressBalances(
            [FromRoute][FromQuery] AddressBalancesRequest request)
        {
            IReadOnlyCollection<AddressBalanceResponce> result;
            if (request.BlockId != null)
            {
                result =  await _addressQueryFacade.GetBalancesByBlockId
                (
                    request.BlockchainType,
                    request.Address,
                    request.BlockId
                );
            }
            else if (request.BlockNumber != null)
            {
                result = await _addressQueryFacade.GetBalancesByBlockNumber
                (
                    request.BlockchainType,
                    request.Address,
                    request.BlockNumber.Value
                );
            }
            else if (request.DateTime != null)
            {
                result = await _addressQueryFacade.GetBalancesOnDate
                (
                    request.BlockchainType,
                    request.Address,
                    request.DateTime.Value.UtcDateTime
                );
            }
            else
            {
                result = (await _addressQueryFacade.GetBalances
                (
                    request.BlockchainType,
                    request.Address
                ));
            }

            return Ok(result);
        }

        [HttpGet(RoutePrefix + "/{address}/unspent-outputs", Name = nameof(GetAddressUnspentOutputs))]
        public async Task<ActionResult<Paginated<AddressUnspentOutputResponce, string>>> GetAddressUnspentOutputs(
            [FromRoute] [FromQuery] AddressUnspentOutputsRequest request)
        {
            var result = await _addressQueryFacade.GetUnspentOutputs
            (
                request.BlockchainType,
                request.Address,
                request.Limit,
                request.Order == PaginationOrder.Asc,
                request.StartingAfter,
                request.EndingBefore
            );

            return result.Paginate(request, Url, p => p.Id);
        }
    }
}
