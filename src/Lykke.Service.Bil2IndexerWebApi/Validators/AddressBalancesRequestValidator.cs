using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    public class AddressBalancesRequestValidator:AbstractValidator<AddressBalancesRequest>
    {
        public AddressBalancesRequestValidator()
        {
            this.ValidateBlockchain(p=>p.BlockchainType);

            RuleFor(p => p.BlockNumber).GreaterThan(p => 0).WithMessage("should be a positive number");
            RuleFor(p => p.BlockNumber).Empty().When(p =>  p.DateTime != null)
                    .WithMessage("should be empty when dateTime is set");
            RuleFor(p => p.BlockNumber).Empty().When(p => p.BlockId != null)
                .WithMessage("should be empty when blockId  is set");

            RuleFor(p => p.BlockId).Empty().When(p => p.BlockNumber != null )
                .WithMessage("should be empty when blockNumber is set");
            RuleFor(p => p.BlockId).Empty().When(p => p.DateTime != null)
                .WithMessage("should be empty when  dateTime is set");

            RuleFor(p => p.DateTime)
                .Empty().When(p => p.BlockNumber != null)
                    .WithMessage("should be empty when blockNumber set");
            RuleFor(p => p.DateTime)
                .Empty().When(p => p.BlockId != null)
                .WithMessage("should be empty when blockId is set");
        }
    }
}
