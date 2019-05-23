using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests.Shared;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    public class ByIdRequestValidator : AbstractValidator<ByIdRequest>
    {
        public ByIdRequestValidator()
        {
            this.ValidateBlockchain(p => p.BlockchainType);
        }
    }
}
