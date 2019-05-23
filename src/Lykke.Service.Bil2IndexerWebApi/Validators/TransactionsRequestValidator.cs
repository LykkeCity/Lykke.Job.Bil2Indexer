using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    public class TransactionsRequestValidator:AbstractValidator<TransactionsRequest>
    {
        public TransactionsRequestValidator()
        {
            this.ValidateBlockchain(p => p.BlockchainType);

            RuleFor(p => p.BlockId)
                .Empty().When(p => p.BlockNumber != null)
                .WithMessage("should be empty when blockNumber is set");
            RuleFor(p => p.BlockId)
                .Empty().When(p => p.Address != null)
                .WithMessage("should be empty when address is set");

            RuleFor(p => p.BlockNumber)
                .Empty().When(p => p.BlockId != null)
                .WithMessage("should be empty when blockId is set");
            RuleFor(p => p.BlockNumber)
                .Empty().When(p => p.Address != null)
                .WithMessage("should be empty when address is set");

            RuleFor(p => p.Address)
                .NotEmpty().When(p => p.BlockId == null && p.BlockNumber == null)
                .WithMessage("either blockId, blockNumber or address should be set");
        }
    }
}
