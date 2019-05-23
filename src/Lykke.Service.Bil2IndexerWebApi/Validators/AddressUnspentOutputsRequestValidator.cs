using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    public class AddressUnspentOutputsRequestValidator : AbstractValidator<AddressUnspentOutputsRequest>
    {
        public AddressUnspentOutputsRequestValidator()
        {
            this.ValidateBlockchain(p => p.BlockchainType);

            RuleFor(p => p.Limit).GreaterThan(0);
        }
    }
}
