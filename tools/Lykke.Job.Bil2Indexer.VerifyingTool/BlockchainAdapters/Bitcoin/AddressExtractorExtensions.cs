using System;
using System.Linq;
using NBitcoin;

namespace Lykke.Job.Bil2Indexer.VerifyingTool.BlockchainAdapters.Bitcoin
{
    public static class AddressExtractorExtensions
    {
        public static bool IsUnrecognizedAddress(this Script script)
        {
            return script.ToOps().First().Code == OpcodeType.OP_RETURN;
            //https://en.bitcoin.it/wiki/OP_RETURN data transaction without address
        }

        public static string ExtractAddress(this Script script, Network network)
        {
            var common = script.GetDestinationAddress(network);
            if (common != null)
            {
                return common.ToString();
            }

            if (PayToPubkeyTemplate.Instance.CheckScriptPubKey(script))
            {
                return PayToPubkeyTemplate.Instance.ExtractScriptPubKeyParameters(script).GetAddress(network)
                    .ToString();
            }

            if (PayToPubkeyHashTemplate.Instance.CheckScriptPubKey(script))
            {
                return PayToPubkeyHashTemplate.Instance.ExtractScriptPubKeyParameters(script).GetAddress(network)
                    .ToString();
            }

            if (PayToScriptHashTemplate.Instance.CheckScriptPubKey(script))
            {
                return PayToScriptHashTemplate.Instance.ExtractScriptPubKeyParameters(script).GetAddress(network)
                    .ToString();
            }

            if (PayToWitPubKeyHashTemplate.Instance.CheckScriptPubKey(script))
            {
                return PayToWitPubKeyHashTemplate.Instance.ExtractScriptPubKeyParameters(script).GetAddress(network)
                    .ToString();
            }

            if (IsUnrecognizedAddress(script))
            {
                return null;
            }

            throw new FormatException($"Unable to extract address from {script}");
        }
    }
}
