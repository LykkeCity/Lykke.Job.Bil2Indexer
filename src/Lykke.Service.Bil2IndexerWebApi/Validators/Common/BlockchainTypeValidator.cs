using System;
using System.Linq;
using FluentValidation.Resources;
using FluentValidation.Validators;
using Lykke.Service.Bil2IndexerWebApi.Services;

namespace Lykke.Service.Bil2IndexerWebApi.Validators.Common
{
    internal class BlockchainTypeValidator: PropertyValidator
    {
        protected override bool IsValid(PropertyValidatorContext context)
        {
            var btype = context.PropertyValue as string;
            return SupportedBlockchains.List.Any(l =>
                string.Equals(btype, l, StringComparison.InvariantCultureIgnoreCase));
        }

        public BlockchainTypeValidator(IStringSource errorMessageSource) : base(errorMessageSource)
        {
        }

        public BlockchainTypeValidator(string errorMessageResourceName, Type errorMessageResourceType) : base(errorMessageResourceName, errorMessageResourceType)
        {
        }

        public BlockchainTypeValidator(string errorMessage) : base(errorMessage)
        {
        }
    }
}
