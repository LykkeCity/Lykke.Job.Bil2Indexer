using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Lykke.Bil2.Contract.BlocksReader.Events;
using Lykke.Bil2.SharedDomain;
using Lykke.Job.Bil2Indexer.Domain;
using Lykke.Numerics;
using NBitcoin;
using QBitNinja.Client;
using QBitNinja.Client.Models;
using BlockHeader = Lykke.Job.Bil2Indexer.Domain.BlockHeader;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters.Bitcoin
{
    public class BitcoinBlockchainVerifierAdapter : IBlockchainVerifierAdapter
    {
        private readonly QBitNinjaClient _ninjaClient;
        private readonly Network _network;

        public BitcoinBlockchainVerifierAdapter(string ninjaUrl, Network network)
        {
            if (string.IsNullOrEmpty(ninjaUrl))
                throw new ArgumentException("Should not be empty", nameof(ninjaUrl));

            _network = network;
            _ninjaClient = new QBitNinjaClient(ninjaUrl, network);
        }

        public async Task<(IEnumerable<TransferCoinsTransactionExecutedEvent> coinTransfers, IEnumerable<TransactionFailedEvent> failedTransfers)> GetCoinTransactionsForBlockAsync(BigInteger blockNumber)
        {
            var listTransfered = new List<TransferCoinsTransactionExecutedEvent>();

            var block = await _ninjaClient.GetBlock(BlockFeature.Parse(blockNumber.ToString()), false, true);
            var blockHash = block.Block.Header.GetHash().ToString();


            for (int i = 0; i < block.Block.Transactions.Count; i++)
            {
                var tx = block.Block.Transactions[i];

                var transferEvent = new TransferCoinsTransactionExecutedEvent(
                    blockHash,
                    i,
                    tx.GetHash().ToString(),
                    tx.Outputs.AsIndexedOutputs()
                        .Select(vout =>
                        {
                            var addr = AddressExtractorExtensions.ExtractAddress(vout.TxOut.ScriptPubKey, _network);

                            return new ReceivedCoin(
                                (int)vout.N,
                                new Asset(new AssetId("BTC")),
                                new UMoney(new BigInteger(vout.TxOut.Value.ToUnit(MoneyUnit.Satoshi)), 8),
                                addr != null
                                    ? new Address(AddressExtractorExtensions.ExtractAddress(vout.TxOut.ScriptPubKey, _network))
                                    : null);
                        })
                        .ToList(),
                    tx.Inputs.AsIndexedInputs()
                        .Where(p => !p.PrevOut.IsNull)
                        .Select(vin => new CoinId(vin.PrevOut.Hash.ToString(), (int)vin.PrevOut.N))
                        .ToList(),
                    isIrreversible: false
                );

                listTransfered.Add(transferEvent);
            }

            return (listTransfered, new TransactionFailedEvent[] { });
        }

        public Task<(IEnumerable<TransferAmountTransactionExecutedEvent> amountTransfers, IEnumerable<TransactionFailedEvent> failedTransfers)> GetAmountTransactionsForBlockAsync(BigInteger blockNumber)
        {
            throw new NotImplementedException();
        }

        public async Task<BlockHeader> GetBlockAsync(BigInteger blockNumber)
        {
            var block = await _ninjaClient.GetBlock(BlockFeature.Parse(blockNumber.ToString()), false, false);
            var blockHash = block.Block.Header.GetHash().ToString();
            var version = block.Block.Header.Version;
            var size = block.Block.GetSerializedSize();
            var transactionCount = block.Block.Transactions.Count;
            var previousBlockId = block.Block.Header.HashPrevBlock.ToString();

            return new BlockHeader(blockHash,
                version,
                "Bitcoin",
                (long)blockNumber,
                block.Block.Header.BlockTime.DateTime,
                size,
                transactionCount,
                previousBlockId,
                BlockState.Executed
            );
        }
    }
}
