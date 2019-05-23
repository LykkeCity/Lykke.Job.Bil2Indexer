using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    internal class AssetWithAddressRequestValidator : AbstractValidator<AssetWithAddressRequest>
    {
        public AssetWithAddressRequestValidator()
        {
            this.ValidateBlockchain(p => p.BlockchainType);
        }
    }
}
