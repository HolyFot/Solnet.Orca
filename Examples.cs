using Orca;
using Orca.Address;
using Orca.Swap;
using Orca.Accounts;
using Orca.Models;
using Orca.Math;
using Orca.Quotes;
using Orca.Program;
using Orca.Ticks;
using Solnet.Wallet;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Programs.Models;
using Solnet.Programs.Utilities;
using Solnet.Programs.Abstract;
using System.Collections;
using System.Numerics;
using Solnet.Rpc.Core.Http;
using Solnet.Programs;
using Solnet.Programs.Models.NameService;
using Solnet.Rpc.Types;
using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Solnet.Rpc.Messages;
using Orca.Types;

public class Program
{
    const string pubKey = "";
    const string privKey = "";

    public static async Task Main(string[] agrs)
    {
        await SwapTest();
        await OtherTests();

        //OpenPosition & ClosePosition Examples
        //OrcaAPI orcaAPI = new OrcaAPI(pubKey, privKey);
        //string posAccount = await OpenToken2022Position(orcaAPI, "C9U2Ksk6KKWvLEeo5yUQ7Xu46X7NzeBJtd9PBfuXaUSM", -12800, 12800);
        //await Task.Delay(1000);
        //await CloseToken2022Position(orcaAPI, new PublicKey(posAccount), new PublicKey("C9U2Ksk6KKWvLEeo5yUQ7Xu46X7NzeBJtd9PBfuXaUSM"));
    }

    public static async Task SwapTest()
    {
        //Input Info: User wants to swap Fartcoin for WSOL
        PublicKey tokenA = new PublicKey("So11111111111111111111111111111111111111112"); // SOL (Pools seem to start tokenA as SOL)
        PublicKey tokenB = new PublicKey("9BB6NFEcjBCtnNLFko2FqVQBq8HHM13kCyYcdQbgpump"); // Fartcoin
        string poolID = "C9U2Ksk6KKWvLEeo5yUQ7Xu46X7NzeBJtd9PBfuXaUSM"; // Example Pool ID for Fartcoin/SOL
        decimal amountIn = 0.01m; // 1 SOL
        SwapDirection swapDirection = SwapDirection.AtoB; // AtoB direction (SOL->Fartcoin)

        // Create an instance of OrcaAPI
        OrcaAPI orcaAPI = new OrcaAPI(pubKey, privKey);
        int tokenInputDecimals = 6; // Fartcoin has 6 decimals
        BigInteger amountLamports = 0;

        // Grab Decimals & Calculate Lamports
        if (swapDirection == SwapDirection.AtoB)
        {
            var TokenInfo = await orcaAPI.rpcClient.GetTokenMintInfoAsync(tokenA.ToString());
            if (TokenInfo.WasSuccessful && TokenInfo.Result.Value != null)
                tokenInputDecimals = TokenInfo.Result.Value.Data.Parsed.Info.Decimals;
            amountLamports = (BigInteger)(amountIn * (decimal)BigInteger.Pow(10, tokenInputDecimals));
            Console.WriteLine($"Swap amount using Lamports: {amountLamports} of {tokenA}");
        }
        if (swapDirection == SwapDirection.BtoA)
        {
            var TokenInfo = await orcaAPI.rpcClient.GetTokenMintInfoAsync(tokenB.ToString());
            if (TokenInfo.WasSuccessful && TokenInfo.Result.Value != null)
                tokenInputDecimals = TokenInfo.Result.Value.Data.Parsed.Info.Decimals;
            amountLamports = (BigInteger)(amountIn * (decimal)BigInteger.Pow(10, tokenInputDecimals));
            Console.WriteLine($"Swap amount using Lamports: {amountLamports} of {tokenB}");
        }

        // Call the ExecuteSwapAsync method on the orcaAPI instance
        await orcaAPI.ExecuteSwapAsync(
            tokenA,
            tokenB,
            poolID,
            amountLamports,
            swapDirection,
            true     // Unwrap WSOL if needed
        );
    }

    public static async Task<string> OpenToken2022Position(OrcaAPI orcaApi, string poolID, int tickLowerIndex, int tickUpperIndex)
    {
        Console.WriteLine("Attempting to open a Token-2022 position...");

        // 1. Generate a new keypair for the position mint. This will be a Token-2022 mint.
        Account positionMintAccount = new Account();
        Console.WriteLine($"Generated new Position Mint: {positionMintAccount.PublicKey}");

        // 2. Define the funder (usually the OrcaAPI's configured account)
        Account funderAccount = orcaApi.account;
        PublicKey ownerAccount = orcaApi.account.PublicKey;

        // 3. Derive PDAs
        // The position PDA is derived using the position mint
        Pda positionPda = PdaUtils.GetPosition(orcaApi.ctx.ProgramId, positionMintAccount.PublicKey);
        // The metadata PDA for the position mint (Token-2022 NFTs often have metadata)
        Pda metadataPda = PdaUtils.GetPositionMetadata(positionMintAccount.PublicKey);

        Console.WriteLine($"Derived Position PDA: {positionPda.PublicKey}");
        Console.WriteLine($"Derived Metadata PDA: {metadataPda.PublicKey}");
        bool is2022Token = await TokenInfo.IsToken2022(orcaApi.rpcClient, positionMintAccount.PublicKey);

        // 4. Prepare accounts for the instruction
        var openPositionAccounts = new OpenPositionWithTokenExtensionsAccounts
        {
            Funder = funderAccount.PublicKey,
            Owner = ownerAccount,
            Position = positionPda.PublicKey,
            PositionMint = positionMintAccount.PublicKey,
            PositionMetadataAccount = metadataPda.PublicKey,
            // The PositionTokenAccount will be an ATA derived for the owner and the new positionMint
            PositionTokenAccount = TokenInfo.DeriveAssociatedTokenAccount(ownerAccount, positionMintAccount.PublicKey, is2022Token),
            Whirlpool = new PublicKey(poolID),
            Token2022Program = TokenInfo.TOKEN_2022_PROGRAM_ID, // Crucial for Token-2022
            SystemProgram = SystemProgram.ProgramIdKey,
            Rent = SysVars.RentKey,
            AssociatedTokenProgram = AddressConstants.ASSOCIATED_TOKEN_PROGRAM_PUBKEY,
            MetadataProgram = AddressConstants.METADATA_PROGRAM_PUBKEY,
            MetadataUpdateAuth = AddressConstants.METADATA_UPDATE_AUTH_PUBKEY // Or appropriate authority
        };

        // 5. Prepare bumps
        var bumps = new OpenPositionWithMetadataBumps // Reusing bumps from metadata variant
        {
            PositionBump = positionPda.Bump,
            MetadataBump = metadataPda.Bump
        };

        // 6. Create a signing callback. The funder and the new positionMintAccount need to sign.
        var signers = new List<Account> { funderAccount, positionMintAccount };
        SigningCallback signingCallback = new SigningCallback(signers, funderAccount);

        try
        {
            // 7. Send the transaction using WhirlpoolClient
            RequestResult<string> result = await orcaApi.ctx.WhirlpoolClient.SendOpenPositionV2(
                openPositionAccounts,
                bumps,
                tickLowerIndex,
                tickUpperIndex,
                funderAccount.PublicKey, // Fee payer
                signingCallback.Sign,    // Signing callback
                orcaApi.ctx.ProgramId
            );

            if (result.WasSuccessful)
            {
                Console.WriteLine($"Successfully opened Token-2022 position! Signature: {result.Result}");
                return result.Result;
            }
            else
            {
                Console.WriteLine($"Failed to open Token-2022 position: {result.Reason}");
                Console.WriteLine($"Raw RPC Response: {result.RawRpcResponse}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while opening Token-2022 position: {ex.Message}");
            return null;
        }
    }

    public static async Task<string> CloseToken2022Position(OrcaAPI orcaApi, PublicKey positionMintToClose, PublicKey positionAccountKey)
    {
        Console.WriteLine($"Attempting to close Token-2022 position for mint: {positionMintToClose}...");

        // 1. Define accounts
        Account positionAuthorityAccount = orcaApi.account; // The owner of the position
        PublicKey receiverAccount = orcaApi.account.PublicKey; // Account to receive reclaimed lamports

        // The PositionTokenAccount is the ATA for the position owned by the positionAuthority
        PublicKey positionTokenAccount = TokenInfo.DeriveAssociatedTokenAccount(
            positionAuthorityAccount.PublicKey,
            positionMintToClose,  //Mint
            await TokenInfo.IsToken2022(orcaApi.rpcClient, positionMintToClose) //Is 2022 token?
        );

        var closePositionAccounts = new ClosePositionWithTokenExtensionsAccounts
        {
            PositionAuthority = positionAuthorityAccount.PublicKey,
            Receiver = receiverAccount,
            Position = positionAccountKey, // This is the PDA of the Position account itself
            PositionMint = positionMintToClose,
            PositionTokenAccount = positionTokenAccount,
            Token2022Program = TokenInfo.TOKEN_2022_PROGRAM_ID // Crucial
        };

        // 2. Create signing callback (only positionAuthority needs to sign)
        SigningCallback signingCallback = new SigningCallback(new List<Account> { positionAuthorityAccount }, positionAuthorityAccount);

        try
        {
            // 3. Send the transaction using WhirlpoolClient
            RequestResult<string> result = await orcaApi.ctx.WhirlpoolClient.SendClosePositionV2(
                closePositionAccounts,
                positionAuthorityAccount.PublicKey, // Fee payer
                signingCallback.Sign,
                orcaApi.ctx.ProgramId
            );

            if (result.WasSuccessful)
            {
                Console.WriteLine($"Successfully closed Token-2022 position! Signature: {result.Result}");
                return result.Result;
            }
            else
            {
                Console.WriteLine($"Failed to close Token-2022 position: {result.Reason}");
                Console.WriteLine($"Raw RPC Response: {result.RawRpcResponse}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while closing Token-2022 position: {ex.Message}");
            return null;
        }
    }

    public static async Task OtherTests()
    {
        OrcaAPI orcaAPI = new OrcaAPI(pubKey, privKey);
        Console.WriteLine("Orca API Setup.");


        //GET TOKEN INFO
        PublicKey mint = new PublicKey("9BB6NFEcjBCtnNLFko2FqVQBq8HHM13kCyYcdQbgpump");
        PublicKey programID = await TokenInfo.GetTokenProgramId(orcaAPI.rpcClient, mint);
        bool is2022Token = await TokenInfo.IsToken2022(orcaAPI.rpcClient, mint);
        Console.WriteLine($"ProgramID for mint: {programID}");
        Console.WriteLine($"Is 2022 token?: {is2022Token}");


        //QUOTES & POOL DATA
        AccountResultWrapper<Whirlpool> result2 = await orcaAPI.ctx.WhirlpoolClient.GetWhirlpoolAsync("C9U2Ksk6KKWvLEeo5yUQ7Xu46X7NzeBJtd9PBfuXaUSM");
        if (result2.WasSuccessful)
        {
            Whirlpool pool2 = result2.ParsedResult;

            //Quote Example
            Percentage slippage = new Percentage(0.1); //1%
            SwapQuote quote = await SwapQuoteUtils.SwapQuoteByInputToken( // SwapQuoteByOutputToken
                orcaAPI.ctx,
                pool2,
                pool2.Address,
                mint,
                SolHelper.ConvertToLamports(0.01m), //amount
                slippage, //slippage
                orcaAPI.ctx.ProgramId //program ID
            );
            Console.WriteLine($"Quote Out Amount: {quote.EstimatedAmountOut}, direction: {quote.AtoB.ToString()}");

            //Swap With Quote
            //Solnet.Rpc.Models.Transaction transResult = await orcaAPI.dex.SwapWithQuote(
               // pool2.Address,
                //quote,
                //false,
                //Solnet.Rpc.Types.Commitment.Finalized); //AtoB Direction
            //Console.WriteLine(transResult.Signatures[0]);

            //Pool Liquidity
            Console.WriteLine($"Pool liquidity: {pool2.Liquidity}, direction: {quote.AtoB.ToString()}");
        }

        //GET POOL ADDRESS FROM TOKENS
        PublicKey tokenA = new PublicKey("9BB6NFEcjBCtnNLFko2FqVQBq8HHM13kCyYcdQbgpump");
        PublicKey tokenB = new PublicKey("So11111111111111111111111111111111111111112");
        Pool pool1 = await orcaAPI.dex.FindWhirlpoolAddress(tokenA, tokenB); //128, new PublicKey(AddressConstants.WHIRLPOOLS_CONFIG_PROGRAM_ID), Solnet.Rpc.Types.Commitment.Finalized);
        Console.WriteLine($"Get Pool ID: {pool1.Address}");


        //GET PRICE OF POOL
        (decimal price, decimal feesPercent) = await orcaAPI.GetSwapPriceByPoolAsync(pool1.Address, SwapDirection.AtoB);
        Console.WriteLine($"Get Pool Price: {price}, fees: {feesPercent}");


        //GET SWAP PARAMETERS USING QUOTE (MIGHT BE SLOW)
        Percentage slippage2 = new Percentage(0.1); //1%
        TransactionBuilder tns = await orcaAPI.dex.GetSwapInstructions(
            pool1.Address,
            100000, //Amount
            tokenA, //Input Token
            true,   //Amount is in Input Token quantity
            slippage2.ToDouble(), //Slippage
            false,  //Unwrap SOL?
            Solnet.Rpc.Types.Commitment.Finalized //Commitment
        );
        Console.WriteLine($"Got Slow Quote Trans.");

        //BUILD SWAP PARAMETERS (POTENTIALLY FASTER)
        string poolID = "C9U2Ksk6KKWvLEeo5yUQ7Xu46X7NzeBJtd9PBfuXaUSM";
        decimal amount = 1000000;
        AccountResultWrapper<Whirlpool> result3 = await orcaAPI.ctx.WhirlpoolClient.GetWhirlpoolAsync(poolID);
        if (result3.WasSuccessful)
        {

            Whirlpool pool3 = result2.ParsedResult;
            PublicKey[] tickArray = await orcaAPI.GetTickArrayAddresses(pool3, true);
            Pda oraclePda1 = PdaUtils.GetOracle(orcaAPI.ctx.ProgramId, new PublicKey(poolID));
        }

        //POOL UTILS EXAMPLE
        AccountResultWrapper<Whirlpool> result = await orcaAPI.ctx.WhirlpoolClient.GetWhirlpoolAsync("C9U2Ksk6KKWvLEeo5yUQ7Xu46X7NzeBJtd9PBfuXaUSM");
        if (result.WasSuccessful)
        {
            Whirlpool pool = result.ParsedResult;
            TokenType tokenInfo = PoolUtils.GetTokenType(pool, pool.TokenMintA);
            Percentage feeRate = PoolUtils.GetFeeRate(pool.FeeRate);
        }


        //GET PDA ORACLE
        Pda oraclePda = PdaUtils.GetOracle(orcaAPI.ctx.ProgramId, new PublicKey("9BB6NFEcjBCtnNLFko2FqVQBq8HHM13kCyYcdQbgpump"));


        //POOL DATA STREAMING
        //orcaAPI.ctx.WhirlpoolClient.SubscribeWhirlpoolAsync

        //OTHER POOL ADMIN STUFF
        //orcaAPI.ctx.WhirlpoolClient.SendSetProtocolFeeRateAsync
        //orcaAPI.ctx.WhirlpoolClient.SendSetRewardAuthorityAsync
        //orcaAPI.ctx.WhirlpoolClient.SendSetFeeAuthorityAsync
        //orcaAPI.ctx.WhirlpoolClient.SendCollectFeesAsync
        //orcaAPI.ctx.WhirlpoolClient.GetFeeTiersAsync
        //orcaAPI.ctx.WhirlpoolClient.SendIncreaseLiquidityAsync
        //orcaAPI.ctx.WhirlpoolClient.SendDecreaseLiquidityAsync
        //orcaAPI.ctx.WhirlpoolClient.SendInitializePoolAsync
    }
}
