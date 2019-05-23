﻿using FluentValidation;
using Lykke.Service.Bil2IndexerWebApi.Models.Requests;
using Lykke.Service.Bil2IndexerWebApi.Validators.Common;

namespace Lykke.Service.Bil2IndexerWebApi.Validators
{
    public class AssetsRequestValidator:AbstractValidator<AssetsRequest>
    {
        public AssetsRequestValidator()
        {
            this.ValidateBlockchain(p => p.BlockchainType);
        }
    }
}
