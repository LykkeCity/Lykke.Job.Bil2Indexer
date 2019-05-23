using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    public class AssetsRequestValidator:AbstractValidator<AssetsRequest>
    {
        public AssetsRequestValidator()
        {
            this.ValidateBlockchain(p => p.BlockchainType);

            RuleFor(p => p.Pagination.StartingAfter).Empty()
                .When(p => !string.IsNullOrEmpty(p.AssetTicker))
                    .WithMessage("should be empty when assetTicker is set");

            RuleFor(p => p.Pagination.EndingBefore).Empty()
                .When(p => !string.IsNullOrEmpty(p.AssetTicker))
                    .WithMessage("should be empty when assetTicker is set");
        }
    }
}
