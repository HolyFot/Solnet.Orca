using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Solnet.Wallet;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Types;
using Solnet.Rpc.Core.Http;
using Solnet.Programs;
using Solnet.Rpc.Models;

namespace Orca.SolanaApi
{
    /// <summary>
    /// Interacts with the System program. 
    /// </summary>
    public static class SystemUtils
    {
        /// <summary>
        /// Airdrop the given address a given amount of SOL. If no address is given, defaults to the main wallet
        /// in the given WhirlpoolContext.
        /// </summary>
        /// <param name="ctx">Application context object.</param>
        /// <param name="recipient">Optional; the address to which to airdrop SOL.</param>
        /// <param name="lamports">The number of lamports to airdrop; defaults to 2 SOL.</param>
        /// <param name="commitment">Preflight and transaction commitment.</param>
        /// <returns>The RPC client response object.</returns>
        public static async Task<RequestResult<string>> RequestAirdropAsync(
            IWhirlpoolContext ctx, 
            PublicKey recipient = null, 
            ulong lamports = 2000000000,
            Commitment commitment = Commitment.Finalized
        )
        {
            var result = await ctx.RpcClient.RequestAirdropAsync(
                recipient  == null ? ctx.WalletPubKey : recipient,
                lamports, commitment: commitment);

            return result;
        }

        /// <summary>
        /// Transfer a given amount of SOL from one address to another. 
        /// </summary>
        /// <param name="ctx">Application context object.</param>
        /// <param name="from">The address from which to transfer.</param>
        /// <param name="to">The address to which to transfer.</param>
        /// <param name="lamports">The number of lamports to transfer</param>
        /// <param name="commitment">Preflight and transaction commitment.</param>
        /// <returns>The RPC client response object.</returns>
        public static async Task<RequestResult<string>> TransferSolAsync(
            IWhirlpoolContext ctx, 
            Account from, 
            PublicKey to, 
            ulong lamports,
            Commitment commitment = Commitment.Finalized
        )
        {
            var latestBlockHashItem = await ctx.RpcClient.GetLatestBlockHashAsync();
            var tb = new TransactionBuilder()
                .SetRecentBlockHash(latestBlockHashItem.Result.Value.Blockhash)
                .SetFeePayer(from.PublicKey)
                .AddInstruction(SystemProgram.Transfer(from.PublicKey, to, lamports));

            var tx = tb.Build(from); 

            return await ctx.RpcClient.SendTransactionAsync(tx, commitment: commitment);
        }
        
        /// <summary>
        /// Creates a new account owned by the system program. 
        /// </summary>
        /// <param name="ctx">Application context object.</param>
        /// <param name="fromAccount">Account creator.</param>
        /// <param name="newAccount">Keypair for new account.</param>
        /// <param name="feePayer">Account who will pay transaction fees.</param>
        /// <param name="commitment">Preflight and transaction commitment.</param>
        /// <returns>The RPC client response object.</returns>
        public static async Task<RequestResult<string>> CreateAccountAsync(
            IWhirlpoolContext ctx, 
            PublicKey fromAccount,
            Account newAccount,
            Account feePayer, 
            Commitment commitment = Commitment.Finalized
        )
        {
            var latestBlockHashItem = await ctx.RpcClient.GetLatestBlockHashAsync();
            var tx = new TransactionBuilder()
                .SetRecentBlockHash(latestBlockHashItem.Result.Value.Blockhash)
                .SetFeePayer(feePayer)
                .AddInstruction(SystemProgram.CreateAccount(
                    fromAccount: fromAccount,
                    space: TokenProgram.TokenAccountDataSize,
                    newAccountPublicKey: newAccount.PublicKey,
                    lamports: 1_000_000_000,
                    programId: SystemProgram.ProgramIdKey
                )).Build(new List<Account> { newAccount });

            return await ctx.RpcClient.SendTransactionAsync(tx, commitment: commitment);
        }
    }
}