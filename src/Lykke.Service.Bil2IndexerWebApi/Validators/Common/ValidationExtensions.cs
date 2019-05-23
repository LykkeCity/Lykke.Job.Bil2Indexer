using System;
using System.Linq;
using System.Linq.Expressions;
using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Services;

namespace Lykke.Service.Bil2IndexerWebApi.Validators.Common
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> ValidateBlockchain<T>(this AbstractValidator<T> builder, Expression<Func<T, string>> propSelector)
        {
            return builder.RuleFor(propSelector).Must(p =>
            {
                return SupportedBlockchains.List.Any(l =>
                    string.Equals(p, l, StringComparison.InvariantCultureIgnoreCase));
            }).WithMessage("not supported");
        }
    }
}
