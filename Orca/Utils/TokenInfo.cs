using System;
using System.Numerics;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using Solnet.Rpc.Types;
using Orca.Accounts;

namespace Orca
{
    public static class TokenInfo
    {
        public static readonly PublicKey TOKEN_PROGRAM_ID = new PublicKey("TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA");
        public static readonly PublicKey TOKEN_2022_PROGRAM_ID = new PublicKey("TokenzQdBNbLqP5VEhdkAS6EPFLC1PHnBqCXEpPxuEb");

        public static async Task<PublicKey> GetTokenProgramId(IRpcClient connection, PublicKey tokenMint)
        {
            try
            {
                var accountInfo = await connection.GetAccountInfoAsync(tokenMint);

                if (!accountInfo.WasSuccessful || accountInfo.Result?.Value?.Owner == null)
                {
                    Console.WriteLine($"Failed to fetch account info for token mint {tokenMint}");
                    return null;
                }

                var owner = accountInfo.Result.Value.Owner;

                if (owner.Equals(TOKEN_PROGRAM_ID.Key))
                {
                    return TOKEN_PROGRAM_ID;
                }
                else if (owner.Equals(TOKEN_2022_PROGRAM_ID.Key))
                {
                    return TOKEN_2022_PROGRAM_ID;
                }
                else
                {
                    Console.WriteLine($"Token mint {tokenMint} is not owned by a known token program");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting token program ID: {ex.Message}");
                return null;
            }
        }

        public static async Task<bool> IsToken2022(IRpcClient connection, PublicKey tokenMint)
        {
            var programId = await GetTokenProgramId(connection, tokenMint);
            return programId.Equals(TOKEN_2022_PROGRAM_ID);
        }

        public static async Task<(PublicKey TokenProgramA, PublicKey TokenProgramB)> GetTokenProgramsForWhirlpool(
            IRpcClient connection,
            Whirlpool whirlpoolData)
        {
            var tokenProgramA = await GetTokenProgramId(connection, whirlpoolData.TokenMintA);
            var tokenProgramB = await GetTokenProgramId(connection, whirlpoolData.TokenMintB);

            return (tokenProgramA, tokenProgramB);
        }
    }
}