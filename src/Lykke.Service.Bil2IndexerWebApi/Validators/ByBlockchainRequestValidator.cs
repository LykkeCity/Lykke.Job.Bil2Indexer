using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests.Shared;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    internal class ByBlockchainRequestValidator:AbstractValidator<ByBlockchainRequest>
    {
        public ByBlockchainRequestValidator()
        {
            this.ValidateBlockchain(p => p.BlockchainType);
        }
    }
}
