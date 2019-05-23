using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    public class ByBlockHeightRequestValidator : AbstractValidator<ByBlockHeightRequest>
    {
        public ByBlockHeightRequestValidator()
        {
            this.ValidateBlockchain(p => p.BlockchainType);
        }
    }
}
