using Solnet.Wallet;
using Solnet.Rpc;


namespace Orca
{
    public interface IWhirlpoolContext
    {
        /// <summary>
        /// Gets the Whirlpools program ID.
        /// </summary>
        PublicKey ProgramId { get; }

        /// <summary>
        /// Gets an objects that can make curated RPC calls to the Whirlpools program. 
        /// </summary>
        WhirlpoolClient WhirlpoolClient { get;  }

        /// <summary>
        /// Gets an objects that can make raw RPC calls to the Solana blockchain in the current environment. 
        /// </summary>
        IRpcClient RpcClient { get; }
        
        /// <summary>
        /// Gets an objects that can connect to the websocket server for the current environment. 
        /// </summary>
        IStreamingRpcClient StreamingRpcClient { get; }

        /// <summary>
        /// Gets the public key of the Wallet.
        /// </summary>
        PublicKey WalletPubKey { get; }

        /// <summary>
        /// Private Key of the Wallet, used for signing transactions.
        /// </summary>
        Account account { get; }
    }
}