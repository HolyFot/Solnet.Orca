using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using Solnet.Rpc.Types;

public static class TokenInfo
{
    public static readonly PublicKey TOKEN_PROGRAM_ID = new PublicKey("TokenkegQfeZyiNwAJbNbGKPFXCWuBvf9Ss623VQ5DA");
    public static readonly PublicKey TOKEN_2022_PROGRAM_ID = new PublicKey("TokenzQdBNbLqP5VEhdkAS6EPFLC1PHnBqCXEpPxuEb");
    public static readonly PublicKey ASSOCIATED_TOKEN_PROGRAM_ID = new PublicKey("ATokenGPvbdGVxr1b2hvZbsiqW5xWH25efTNsLJA8knL");
    public static readonly string MEMO_PROGRAM_ID = "Memo1UhkJRfHyvLMcVucJwxXeuD728EqVDDwQDxFMNo";
    public static readonly PublicKey MEMO_PROGRAM_PUBKEY = new PublicKey(MEMO_PROGRAM_ID);
    public static readonly PublicKey WSOL_TOKEN = new PublicKey("So11111111111111111111111111111111111111112");

    public static async Task<PublicKey> GetTokenProgramId(IRpcClient connection, PublicKey tokenMint)
    {
        try
        {
            var accountInfo = await connection.GetAccountInfoAsync(tokenMint.ToString()); // Ensure .ToString() for RPC call

            if (!accountInfo.WasSuccessful || accountInfo.Result?.Value?.Owner == null)
            {
                Console.WriteLine($"Failed to fetch account info for token mint {tokenMint}");
                return null;
            }

            var owner = new PublicKey(accountInfo.Result.Value.Owner); // Convert owner string to PublicKey

            if (owner.Equals(TOKEN_PROGRAM_ID))
            {
                return TOKEN_PROGRAM_ID;
            }
            else if (owner.Equals(TOKEN_2022_PROGRAM_ID))
            {
                return TOKEN_2022_PROGRAM_ID;
            }
            else
            {
                Console.WriteLine($"Token mint {tokenMint} is not owned by a known token program. Owner: {owner}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting token program ID for {tokenMint}: {ex.Message}");
            return null;
        }
    }

    public static async Task<bool> IsToken2022(IRpcClient connection, PublicKey tokenMint)
    {
        var programId = await GetTokenProgramId(connection, tokenMint);
        return programId != null && programId.Equals(TOKEN_2022_PROGRAM_ID);
    }

    public static PublicKey DeriveAssociatedTokenAccount(PublicKey owner, PublicKey mint, bool is2022Token)
    {
        PublicKey tokenProgramToUse = is2022Token ? TOKEN_2022_PROGRAM_ID : TOKEN_PROGRAM_ID;

        // Correct derivation using Solnet's utility or manual PDA finding:
        if (PublicKey.TryFindProgramAddress(
           new List<byte[]>
           {
                    owner.KeyBytes,
                    TOKEN_PROGRAM_ID.KeyBytes, // Standard SPL Token Program ID is always used for ATA derivation seed
                    mint.KeyBytes
           },
           ASSOCIATED_TOKEN_PROGRAM_ID,
           out var address, out var _))
        {
            return address;
        }
        return null;
    }

    public static TransactionInstruction CreateAssociatedTokenAccount(PublicKey payer, PublicKey owner, PublicKey mint, bool is2022Token)
    {
        PublicKey derivedAta = DeriveAssociatedTokenAccount(owner, mint, is2022Token);
        if (derivedAta == null)
        {
            Console.WriteLine($"Could not derive ATA for owner {owner} and mint {mint}");
            return null;
        }

        PublicKey tokenProgramToUse = is2022Token ? TOKEN_2022_PROGRAM_ID : TOKEN_PROGRAM_ID;

        List<AccountMeta> keys = new List<AccountMeta>
            {
                AccountMeta.Writable(payer, isSigner: true),
                AccountMeta.Writable(derivedAta, isSigner: false),
                AccountMeta.ReadOnly(owner, isSigner: false),
                AccountMeta.ReadOnly(mint, isSigner: false),
                AccountMeta.ReadOnly(SystemProgram.ProgramIdKey, isSigner: false),
                AccountMeta.ReadOnly(tokenProgramToUse, isSigner: false), // Correctly use determined token program
                // Rent sysvar is no longer explicitly required for ATA creation in recent Solana versions / Solnet SDK,
                // but if your specific Solnet version or program interaction needs it, uncomment:
                // AccountMeta.ReadOnly(SysVars.RentKey, isSigner: false) 
            };

        return new TransactionInstruction
        {
            ProgramId = ASSOCIATED_TOKEN_PROGRAM_ID.KeyBytes,
            Keys = keys,
            Data = Array.Empty<byte>() // CreateAssociatedTokenAccount instruction has no data
        };
    }
}
