using Solnet.Rpc.Types;
using Solnet.Wallet;
using Solnet;

namespace Orca
{
    public static class OrcaConfiguration
    {
        /// <summary>
        /// Set what environment you'd like tests to run in. 
        /// </summary>
        public static string SolanaEnvironment => SolEnv.MainNetHelius;

        /// <summary>
        /// This only needs to be set if running tests on LocalNet (see SolanaEnvironment). 
        /// This is the localnet address of the Whirlpools program (where you deployed it). 
        /// </summary>
        public static PublicKey LocalNetWhirlpoolAddress => new("whirLbMiicVdio4qvUfM5KAg6Ct8VwpYzGff3uctyCc");

        /// <summary>
        /// This commitment will be used by default (unless otherwise explicitly specified in a call) for each transaction 
        /// and preflight simulation called through WhirlpoolClient. 
        /// </summary>
        public static Commitment DefaultCommitment => Commitment.Confirmed;
    }

}
