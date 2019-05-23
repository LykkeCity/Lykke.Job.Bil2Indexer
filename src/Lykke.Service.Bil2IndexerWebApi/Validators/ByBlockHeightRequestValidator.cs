using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    internal class ByBlockHeightRequestValidator : AbstractValidator<ByBlockNumberRequest>
    {
        public ByBlockHeightRequestValidator()
        {
            this.ValidateBlockchain(p => p.BlockchainType);
        }
    }
}
