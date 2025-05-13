using Orca;
using Orca.Address;
using Orca.Swap;
using Orca.Accounts;
using Orca.Ticks;
using Orca.Math;
using Orca.Program;
using Orca.TxApi;
using Orca.Models;
using Solnet;
using Solnet.Wallet;
using Solnet.Rpc;
using Solnet.Rpc.Types;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Programs.Models;
using Solnet.Programs.Utilities;
using Solnet.Programs;
using Newtonsoft.Json.Linq;
using System.Numerics;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;

namespace Orca
{
    public class OrcaAPI
    {
        public IRpcClient rpcClient;
        public IStreamingRpcClient wsClient;
        private HttpClient _httpClient;
        public WhirlpoolContext ctx;
        public Dex dex;
        public Account account;
        public PublicKey _publicKey;

        #region Constructor/Initializers
        public OrcaAPI(string publicKey, string PrivateKey)
        {
            ctx = CreateWhirlpoolContext(SolEnv.MainNet, publicKey, PrivateKey);
            //ctx = CreateWhirlpoolContext(SolEnv.MainNetHelius, publicKey, PrivateKey);
            dex = new OrcaDex(ctx); 
            _httpClient = CreateDefaultHttpClient();
            _publicKey = new PublicKey(publicKey);
        }

        private WhirlpoolContext CreateWhirlpoolContext(string env, string publicKey, string PrivKey)
        {
            PublicKey programId = AddressConstants.WHIRLPOOLS_PUBKEY;
            rpcClient = ClientFactory.GetClient(env);
            wsClient = ClientFactory.GetStreamingClient(SolEnv.WSMainNet.ToString());
            account = new Account(PrivKey, publicKey);

            return new WhirlpoolContext(
                programId,
                rpcClient,
                wsClient,
                new PublicKey(publicKey),
                account,
                OrcaConfiguration.DefaultCommitment
            );
        }

        private static HttpClient CreateDefaultHttpClient()
        {
            var handler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(3),
                PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
                MaxConnectionsPerServer = 30,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate, // Enable compression handling
                EnableMultipleHttp2Connections = true, // Improve HTTP/2 performance

                // KeepAlive Settings
                KeepAlivePingDelay = TimeSpan.FromSeconds(60), // Use KeepAlivePingDelay for .NET Core
                KeepAlivePingTimeout = TimeSpan.FromSeconds(10), // Use KeepAlivePingTimeout
                KeepAlivePingPolicy = HttpKeepAlivePingPolicy.WithActiveRequests // Only send pings when requests are active
                // Note: EnableTcpKeepAlive is implicitly handled by setting KeepAlivePingDelay/Timeout in newer .NET versions
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30) // Use the reduced default timeout
            };
            client.DefaultRequestHeaders.Add("User-Agent", "ArbitrageBot/1.0");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate"); // Signal we accept compression

            return client;
        }
        #endregion

        #region Swapping & Pricing
        public async Task<(decimal price, decimal feePercent)> GetSwapPriceByPoolAsync(string _poolAddress, SwapDirection direction)
        {
            string url = $"https://api.orca.so/v2/solana/pools/{_poolAddress}";

            HttpResponseMessage response = await _httpClient.GetAsync(url);
            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"ERROR: Failed to get OrcaPriceData: {response.StatusCode}. Response: {json}");
                return (0, 0);
            }

            // Parse JSON response using JObject
            JObject root = JObject.Parse(json);
            JObject data = (JObject)root["data"];
            if (data == null)
            {
                Console.WriteLine("ERROR: JSON does not contain 'data' field.");
                return (0, 0);
            }

            // Extract necessary fields; adjust property names if JSON structure differs
            decimal price = data.Value<decimal>("price");
            decimal feeRate = data.Value<decimal>("feeRate");
            decimal protocolFeeRate = data.Value<decimal>("protocolFeeRate");

            if (price == 0) //Error Couldnt Get Price
                return (0, 0);

            // Calculate price based on swap direction
            decimal priceReversed = 1m / price;
            decimal returnPrice = (direction == SwapDirection.AtoB) ? price : priceReversed;

            // Calculate total fee percent; assuming fee percentages are expressed such that 10000 equals 100%
            decimal totalFees = (feeRate + protocolFeeRate) / 10000m;

            return (returnPrice, totalFees);
        }

        public async Task<TransactionInstruction> GetSwapInstructions(WhirlpoolContext ctx, SwapParams swapParams, bool useV2, Account feePayer = null)
        {
            if (feePayer == null)
                feePayer = account; // Default to instance account if feePayer is null

            if (useV2 && swapParams.AccountsV2 != null)
            {
                // Note: GetSwapV2InstructionsAsync doesn't require feePayer directly, it's for building the instruction
                return await ctx.WhirlpoolClient.GetSwapV2InstructionsAsync(
                    swapParams.AccountsV2,
                    programId: ctx.ProgramId,
                    amount: (ulong)swapParams.Amount,
                    otherAmountThreshold: (ulong)swapParams.OtherThresholdAmount,
                    sqrtPriceLimit: swapParams.SqrtPriceLimit,
                    amountSpecifiedIsInput: swapParams.AmountSpecifiedIsInput,
                    aToB: swapParams.AtoB
                // supplementalTickArrays can be added here if needed
                );
            }
            else if (!useV2 && swapParams.Accounts != null)
            {
                return await ctx.WhirlpoolClient.GetSwapInstructionsAsync(
                   swapParams.Accounts,
                   programId: ctx.ProgramId,
                   amount: (ulong)swapParams.Amount,
                   otherAmountThreshold: (ulong)swapParams.OtherThresholdAmount,
                   sqrtPriceLimit: swapParams.SqrtPriceLimit,
                   amountSpecifiedIsInput: swapParams.AmountSpecifiedIsInput,
                   aToB: swapParams.AtoB,
                   feePayer: feePayer.PublicKey // GetSwapInstructionsAsync (V1) might still expect feePayer
               );
            }
            else
            {
                throw new ArgumentException("SwapParams accounts do not match the specified version (useV2).");
            }
        }
        
        public async Task<bool> ExecuteSwapAsync(PublicKey tokenA,PublicKey tokenB,
            string poolID, BigInteger amountLamports, SwapDirection swapDirection, bool useUnwrapWSOL)
        {
            var startTime = Stopwatch.GetTimestamp();
            TransactionBuilder transBuilder = new TransactionBuilder();
            bool actualAtoB = false;
            
            // 1. Get Pool Info first
            AccountResultWrapper<Whirlpool> poolResult = await this.ctx.WhirlpoolClient.GetWhirlpoolAsync(poolID);
            Whirlpool pool = poolResult.ParsedResult;
            PublicKey poolTokenMintA = pool.TokenMintA;
            PublicKey poolTokenMintB = pool.TokenMintB;

            Console.WriteLine($"Pool's TokenA: {poolTokenMintA}, TokenB: {poolTokenMintB}");
            if (!poolResult.WasSuccessful || poolResult.ParsedResult == null)
            {
                Console.WriteLine($"Failed to get pool info for {poolID}: {poolResult.ParsedResult}");
                return false;
            }

            // 2. Check if the pool is valid for the given tokens
            if (!tokenA.Equals(poolTokenMintA) && !tokenA.Equals(poolTokenMintB))
            {
                Console.WriteLine($"Error: Incorrect Pool for TokenA ({poolTokenMintA}) and TokenB ({poolTokenMintB}), pool: {poolID}.");
                return false;
            }
            if (!tokenB.Equals(poolTokenMintA) && !tokenB.Equals(poolTokenMintB))
            {
                Console.WriteLine($"Error: Incorrect Pool for TokenA ({poolTokenMintA}) and TokenB ({poolTokenMintB}), pool: {poolID}.");
                return false;
            }
            if (swapDirection == SwapDirection.AtoB) actualAtoB = true;
            if (swapDirection == SwapDirection.BtoA) actualAtoB = false;

            // 3. Get ATA's & Token Program IDs
            bool poolTokenAis2022 = await TokenInfo.IsToken2022(this.rpcClient, poolTokenMintA);
            bool poolTokenBis2022 = await TokenInfo.IsToken2022(this.rpcClient, poolTokenMintB);
            bool useV2Swap = poolTokenAis2022 || poolTokenBis2022;

            PublicKey userPoolTokenA_ATA = TokenInfo.DeriveAssociatedTokenAccount(this.account.PublicKey, poolTokenMintA, poolTokenAis2022);
            PublicKey userPoolTokenB_ATA = TokenInfo.DeriveAssociatedTokenAccount(this.account.PublicKey, poolTokenMintB, poolTokenBis2022);
            PublicKey tokenProgramIdA = poolTokenAis2022 ? TokenInfo.TOKEN_2022_PROGRAM_ID : TokenInfo.TOKEN_PROGRAM_ID;
            PublicKey tokenProgramIdB = poolTokenBis2022 ? TokenInfo.TOKEN_2022_PROGRAM_ID : TokenInfo.TOKEN_PROGRAM_ID;

            // 4. Get Tick Arrays & Oracle
            PublicKey[] tickArrays = await this.GetTickArrayAddresses(pool, actualAtoB);
            if (tickArrays == null || tickArrays.Length < 3)
            {
                Console.WriteLine("Error: Could not fetch all required tick arrays for the swap.");
                return false;
            }
            Pda oraclePda = PdaUtils.GetOracle(this.ctx.ProgramId, new PublicKey(poolID));

            // 5a. Create ATA for Pool's Token A if needed
            var inputAtaInfoA = await this.rpcClient.GetAccountInfoAsync(userPoolTokenA_ATA.ToString());
            if (!inputAtaInfoA.WasSuccessful || inputAtaInfoA.Result?.Value == null)
            {
                //Console.WriteLine($"User ATA for Pool Token A ({userPoolTokenA_ATA}) not found. Adding create instruction.");
                transBuilder.AddInstruction(
                    TokenInfo.CreateAssociatedTokenAccount(
                        this.account.PublicKey,
                        this.account.PublicKey,
                        poolTokenMintA,
                        poolTokenAis2022
                    )
                );
            }

            // 5b. Create ATA for Pool's Token B if needed
            var inputAtaInfoB = await this.rpcClient.GetAccountInfoAsync(userPoolTokenB_ATA.ToString());
            if (!inputAtaInfoB.WasSuccessful || inputAtaInfoB.Result?.Value == null)
            {
                //Console.WriteLine($"User ATA for Pool Token B ({userPoolTokenB_ATA}) not found. Adding create instruction.");
                transBuilder.AddInstruction(
                    TokenInfo.CreateAssociatedTokenAccount(
                        this.account.PublicKey,
                        this.account.PublicKey,
                        poolTokenMintB,
                        poolTokenBis2022
                    )
                );
            }

            // 6. Wrap SOL if inputTokenMint is WSOL
            if (useUnwrapWSOL)
            {
                if ((swapDirection == SwapDirection.AtoB && tokenA.Equals(TokenInfo.WSOL_TOKEN))
                    || (swapDirection == SwapDirection.BtoA && tokenB.Equals(TokenInfo.WSOL_TOKEN)))
                {
                    WrapSol(ref transBuilder, (ulong)amountLamports);
                }
            }

            // 7. Get the swap parameters
            SwapParams swapParams = this.GenerateSwapParams(
                ctx: this.ctx,
                tokenOwnerAccountA: userPoolTokenA_ATA,
                tokenOwnerAccountB: userPoolTokenB_ATA,
                tokenMintA: poolTokenMintA,
                tokenMintB: poolTokenMintB,
                tokenVaultA: pool.TokenVaultA,
                tokenVaultB: pool.TokenVaultB,
                whirlpoolAddress: pool.Address,
                tickArrays: tickArrays,
                oracleAddress: oraclePda.PublicKey,
                useV2: useV2Swap,
                tokenProgramIdA: tokenProgramIdA,
                tokenProgramIdB: tokenProgramIdB,
                amount: amountLamports,
                otherThresholdAmount: BigInteger.Zero,
                sqrtPriceLimit: BigInteger.Zero,
                amountSpecifiedIsInput: true,
                aToB: actualAtoB
            );

            // 8a. Get the Block Hash
            var initialBlockhash = await this.rpcClient.GetLatestBlockHashAsync();
            if (!initialBlockhash.WasSuccessful)
            {
                Console.WriteLine("Error: Could not fetch latest blockhash.");
                return false;
            }

            // 8b. Add the swap instruction
            TransactionInstruction swapInstruction = await this.GetSwapInstructions(this.ctx, swapParams, useV2Swap, this.account);
            transBuilder.AddInstruction(swapInstruction);

            // 9. Unwrap WSOL if needed
            if (useUnwrapWSOL)
            {
                // Check if output token is WSOL
                if ((swapDirection == SwapDirection.AtoB && tokenB.Equals(TokenInfo.WSOL_TOKEN))
                    || (swapDirection == SwapDirection.BtoA && tokenA.Equals(TokenInfo.WSOL_TOKEN)))
                {
                    UnWrapSol(ref transBuilder);
                }
            }

            // 10. Build & Sign the transaction
            transBuilder.SetFeePayer(this.account)
                        .SetRecentBlockHash(initialBlockhash.Result.Value.Blockhash)
                        .AddInstruction(ComputeBudgetProgram.SetComputeUnitLimit(600_000));
            var transactionBytes = transBuilder.Build(this.account);

            // 11. Simulate Transaction
            Console.WriteLine("Simulating transaction...");
            var simResult = await this.rpcClient.SimulateTransactionAsync(transactionBytes);
            if (!simResult.WasSuccessful || (simResult.Result?.Value?.Error != null))
            {
                Console.WriteLine($"Transaction simulation failed: {simResult.RawRpcResponse}");
                if (simResult.Result?.Value?.Logs != null)
                {
                    Console.WriteLine($"Simulation Logs:\n{string.Join("\n", simResult.Result.Value.Logs)}");
                }
                return false;
            }
            else
            {
                Console.WriteLine("Transaction simulation successful.");
                //if (simResult.Result?.Value?.Logs != null)
                    //Console.WriteLine($"Simulation Logs:\n{string.Join("\n", simResult.Result.Value.Logs)}");
            }

            // 12. Send Transaction
            var networkTime = Stopwatch.GetTimestamp();
            Console.WriteLine("Sending transaction...");
            var txResult = await this.rpcClient.SendTransactionAsync(transactionBytes, skipPreflight: true, commitment: Commitment.Confirmed);
            if (txResult.WasSuccessful)
            {
                TimeSpan elapsedTime = Stopwatch.GetElapsedTime(startTime);
                TimeSpan elapsedTime2 = Stopwatch.GetElapsedTime(networkTime);
                Console.WriteLine($"Transaction successful! (total time: {elapsedTime.Seconds}sec {elapsedTime.Milliseconds}ms, network time: {elapsedTime2.Seconds}sec {elapsedTime2.Milliseconds}ms)");
                Console.WriteLine($"View on Explorer: https://solscan.io/tx/{txResult.Result}");
                return true;
            }
            else
            {
                Console.WriteLine($"Transaction Failed. Reason: {txResult.Reason}");
                Console.WriteLine($"Raw RPC Response: {txResult.RawRpcResponse}");
                return false;
            }
        }
        #endregion

        #region Wrapping & Unwrapping SOL
        public bool WrapSol(ref TransactionBuilder txBuilder, ulong lamports)
        {
            try
            {
                Console.WriteLine("Adding instruction to Wrap SOL");
                PublicKey wsolAtaForSwapInput = TokenInfo.DeriveAssociatedTokenAccount(this.account.PublicKey, TokenInfo.WSOL_TOKEN, false);

                // 1. Transfer SOL to WSOL ATA
                txBuilder.AddInstruction(
                    SystemProgram.Transfer(
                        this.account.PublicKey,   // From user's main SOL account
                        wsolAtaForSwapInput,      // To user's WSOL ATA
                        (ulong)lamports + 10      // Amount to wrap
                    )
                );
                // 2. Sync the native account to update WSOL balance
                txBuilder.AddInstruction(
                    TokenProgram.SyncNative(
                        wsolAtaForSwapInput
                    )
                );

                return true;
            }
            catch
            {
                Console.WriteLine("Error: Failed to wrap SOL.");
                return false;
            }
        }

        public bool UnWrapSol(ref TransactionBuilder txBuilder)
        {
            try
            {
                PublicKey userWsolAta = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(this.account.PublicKey, TokenInfo.WSOL_TOKEN);
                Console.WriteLine("Adding instruction to unwrap WSOL to native SOL");
                var closeTokenAccountIx = TokenProgram.CloseAccount(
                    userWsolAta,              // Token account to close
                    this.account.PublicKey,   // Destination for recovered SOL
                    this.account.PublicKey,   // Owner of token account
                    TokenProgram.ProgramIdKey // Token program
                );
                txBuilder.AddInstruction(closeTokenAccountIx);

                return true;
            }
            catch
            {
                Console.WriteLine("Error: Failed to UnWrap SOL.");
                return false;
            }
        }
        #endregion

        #region Utils & Params
        /// <summary>
        /// Generates SwapParams, populating either V1 or V2 accounts based on useV2 flag.
        /// </summary>
        public SwapParams GenerateSwapParams(
            WhirlpoolContext ctx,
            PublicKey tokenOwnerAccountA, // User's ATA for token A
            PublicKey tokenOwnerAccountB, // User's ATA for token B
            PublicKey tokenMintA,         // Mint of token A
            PublicKey tokenMintB,         // Mint of token B
            PublicKey tokenVaultA,        // Whirlpool's vault for token A
            PublicKey tokenVaultB,        // Whirlpool's vault for token B
            PublicKey whirlpoolAddress,
            PublicKey[] tickArrays,       // Expects 3 tick arrays
            PublicKey oracleAddress,
            bool useV2,                   // Flag to determine which accounts structure to use
            PublicKey tokenProgramIdA,    // Token program for token A (SPL or Token-2022)
            PublicKey tokenProgramIdB,    // Token program for token B (SPL or Token-2022)
            BigInteger? amount = null,
            BigInteger? otherThresholdAmount = null,
            BigInteger? sqrtPriceLimit = null,
            bool amountSpecifiedIsInput = true,
            bool aToB = true
        )
        {
            if (tickArrays == null || tickArrays.Length < 3)
            {
                throw new ArgumentException("At least 3 tick arrays must be provided.");
            }

            var swapParams = new SwapParams
            {
                Amount = amount ?? BigInteger.Zero,
                OtherThresholdAmount = otherThresholdAmount ?? BigInteger.Zero,
                SqrtPriceLimit = sqrtPriceLimit ?? BigInteger.Zero,
                AmountSpecifiedIsInput = amountSpecifiedIsInput,
                AtoB = aToB
            };

            if (useV2)
            {
                swapParams.AccountsV2 = new SwapV2Accounts
                {
                    TokenProgramA = tokenProgramIdA,
                    TokenProgramB = tokenProgramIdB,
                    MemoProgram = TokenInfo.MEMO_PROGRAM_PUBKEY, // Assuming AddressConstants.MEMO_PROGRAM_PUBKEY exists
                    TokenAuthority = ctx.WalletPubKey,
                    Whirlpool = whirlpoolAddress,
                    TokenMintA = tokenMintA,
                    TokenMintB = tokenMintB,
                    TokenOwnerAccountA = tokenOwnerAccountA,
                    TokenVaultA = tokenVaultA,
                    TokenOwnerAccountB = tokenOwnerAccountB,
                    TokenVaultB = tokenVaultB,
                    TickArray0 = tickArrays[0],
                    TickArray1 = tickArrays[1],
                    TickArray2 = tickArrays[2],
                    Oracle = oracleAddress
                };
            }
            else
            {
                // V1 SwapAccounts typically doesn't need individual token programs or mints directly in its struct,
                // as they are often part of the Whirlpool account data or inferred.
                // It expects a single TokenProgram for both.
                // We'll assume the primary token program (e.g., SPL Token) for V1 if not Token-2022.
                // If one of the tokens is Token-2022, V2 should be used.
                if (tokenProgramIdA.Equals(TokenInfo.TOKEN_2022_PROGRAM_ID) || tokenProgramIdB.Equals(TokenInfo.TOKEN_2022_PROGRAM_ID))
                {
                    // This case should ideally force useV2 = true upstream.
                    // For safety, one might throw or default to a common SPL token program if strictly V1.
                    Console.WriteLine("Warning: Attempting to use V1 swap with Token-2022 mints. This might be unintended.");
                }

                swapParams.Accounts = new SwapAccounts
                {
                    // V1 typically uses one TokenProgram for both sides.
                    // If your V1 expects specific token programs, adjust accordingly.
                    TokenProgram = tokenProgramIdA.Equals(TokenInfo.TOKEN_2022_PROGRAM_ID) ? tokenProgramIdB : tokenProgramIdA,
                    TokenAuthority = ctx.WalletPubKey,
                    Whirlpool = whirlpoolAddress,
                    TokenOwnerAccountA = tokenOwnerAccountA,
                    TokenVaultA = tokenVaultA,
                    TokenOwnerAccountB = tokenOwnerAccountB,
                    TokenVaultB = tokenVaultB,
                    TickArray0 = tickArrays[0],
                    TickArray1 = tickArrays[1],
                    TickArray2 = tickArrays[2],
                    Oracle = oracleAddress
                };
            }
            return swapParams;
        }

        public async Task<IList<TickArrayContainer>> GetTickArrayData(Whirlpool pool, bool AtoB)
        {
            int currentTick = pool.TickCurrentIndex;
            ushort tickSpacing = pool.TickSpacing;
            byte[] speed = pool.TickSpacingSeed;
            IList<TickArrayContainer> tickArrays = await SwapUtils.GetTickArrays(
                ctx,
                currentTick,
                tickSpacing,
                AtoB, //AtoB Direction
                ctx.ProgramId,
                pool.Address
            );
            return tickArrays;
        }

        public async Task<PublicKey[]> GetTickArrayAddresses(Whirlpool pool, bool AtoB)
        {
            int currentTick = pool.TickCurrentIndex;
            ushort tickSpacing = pool.TickSpacing;
            byte[] speed = pool.TickSpacingSeed;
            IList<TickArrayContainer> tickArrayData = await SwapUtils.GetTickArrays(
                ctx,
                currentTick,
                tickSpacing,
                AtoB, //AtoB Direction
                ctx.ProgramId,
                pool.Address
            );
            if (tickArrayData == null || tickArrayData.Count == 0)
            {
                Console.WriteLine("No tick array data found for the given pool.");
                return null;
            }
            // Create a new PublicKey[] array and populate it with the addresses from tickArrayData
            PublicKey[] tickArrays = new PublicKey[tickArrayData.Count];
            for (int i = 0; i < tickArrayData.Count; i++)
            {
                tickArrays[i] = tickArrayData[i].Address;
            }
            return tickArrays;
        }
        #endregion
    }
}