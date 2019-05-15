﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Service.Bil2IndexerWebApi.Models;

namespace Lykke.Service.Bil2IndexerWebApi.Services
{
    public class TransactionQueryFacade: ITransactionQueryFacade
    {
        public Task<Transaction> GetTransactionById(string blockchainType, string id)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<TransactionModel>> GetTransactionsByBlockId(string blockchainType, string blockId, int limit, bool orderAsc, string startingAfter,
            string endingBefore)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<TransactionModel>> GetTransactionsByAddress(string blockchainType, string address, int limit, bool orderAsc, string startingAfter,
            string endingBefore)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyCollection<TransactionModel>> GetTransactionsByBlockNumber(string blockchainType, int blockNumberValue, int limit, bool orderAsc,
            string startingAfter, string endingBefore)
        {
            throw new NotImplementedException();
        }
    }
}
