using Orca;
using Orca.Address;
using Orca.Swap;
using Orca.Accounts;
using Orca.Ticks;
using Orca.Math;
using Orca.Program;
using Solnet.Wallet;
using Solnet.Rpc;
using Solnet.Rpc.Types;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Builders;
using System.Collections.Generic;
using System.Numerics;
using Orca.TxApi;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using Solnet.Rpc.Models;
using Orca.Models;

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

        #region Initializers
        public OrcaAPI(string publicKey, string PrivateKey)
        {
            ctx = CreateWhirlpoolContext(SolEnv.MainNet, publicKey, PrivateKey);
            dex = new OrcaDex(ctx); 
            _httpClient = CreateDefaultHttpClient();
            _publicKey = new PublicKey(publicKey);
            //ctx = CreateWhirlpoolContext(SolEnv.MainNetHelius, BotConfig.PublicKey, BotConfig.PrivateKey);
        }

        private WhirlpoolContext CreateWhirlpoolContext(string env, string publicKey, string PrivKey)
        {
            PublicKey programId = AddressConstants.WHIRLPOOLS_PUBKEY;
            rpcClient = ClientFactory.GetClient("https://api.mainnet-beta.solana.com");
            wsClient = ClientFactory.GetStreamingClient(env.ToString());
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

        public async Task<RequestResult<string>> SwapAsync(WhirlpoolContext ctx, SwapParams swapParams, Account feePayer = null)
        {
            if (feePayer == null)
                feePayer = account;

            SigningCallback signer = new SigningCallback(account, feePayer);

            return await ctx.WhirlpoolClient.SendSwapAsync(
                swapParams.Accounts,
                programId: ctx.ProgramId,
                amount: (ulong)swapParams.Amount,
                otherAmountThreshold: (ulong)swapParams.OtherThresholdAmount,
                sqrtPriceLimit: swapParams.SqrtPriceLimit,
                amountSpecifiedIsInput: swapParams.AmountSpecifiedIsInput,
                aToB: swapParams.AtoB,
                feePayer: feePayer.PublicKey,
                signingCallback: (byte[] msg, PublicKey _publicKey) => signer.Sign(msg, account)
            );
        }

        public async Task<TransactionInstruction> GetSwapInstructions(WhirlpoolContext ctx, SwapParams swapParams, Account feePayer = null)
        {
            if (feePayer == null)
                feePayer = account;

            return await ctx.WhirlpoolClient.GetSwapInstructionsAsync(
                swapParams.Accounts,
                programId: ctx.ProgramId,
                amount: (ulong)swapParams.Amount,
                otherAmountThreshold: (ulong)swapParams.OtherThresholdAmount,
                sqrtPriceLimit: swapParams.SqrtPriceLimit,
                amountSpecifiedIsInput: swapParams.AmountSpecifiedIsInput,
                aToB: swapParams.AtoB,
                feePayer: feePayer.PublicKey
            );
        }
        #endregion

        #region Utils & Params
        public SwapParams GenerateSwapParams(
            WhirlpoolContext ctx,
            PublicKey tokenAccountA,
            PublicKey tokenAccountB,
            PublicKey tokenVaultA,
            PublicKey tokenVaultB,
            PublicKey whirlpoolAddress,
            PublicKey[] tickArrays,
            PublicKey oracleAddress,
            BigInteger? amount = null,
            BigInteger? otherThresholdAmount = null,
            BigInteger? sqrtPriceLimit = null,
            bool amountSpecifiedIsInput = true,
            bool aToB = true
        )
        {
            SwapAccounts accounts = new SwapAccounts
            {
                TokenProgram = AddressConstants.TOKEN_PROGRAM_PUBKEY,
                TokenAuthority = ctx.WalletPubKey,
                Whirlpool = whirlpoolAddress,
                TokenOwnerAccountA = tokenAccountA,
                TokenVaultA = tokenVaultA,
                TokenOwnerAccountB = tokenAccountB,
                TokenVaultB = tokenVaultB,
                TickArray0 = tickArrays[0],
                TickArray1 = tickArrays[1],
                TickArray2 = tickArrays[2],
                Oracle = oracleAddress
            };

            return new SwapParams
            {
                Accounts = accounts,
                Amount = amount != null ? amount.Value : 0,
                OtherThresholdAmount = otherThresholdAmount != null ? otherThresholdAmount.Value : 0,
                SqrtPriceLimit = sqrtPriceLimit != null ? sqrtPriceLimit.Value : 0,
                AmountSpecifiedIsInput = amountSpecifiedIsInput,
                AtoB = aToB
            };
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

    public static class SolEnv
    {
        public const string LocalNet = "127.0.0.1";
        public const string DevNet = "https://api.devnet.solana.com";
        public const string TestNet = "https://api.testnet.solana.com";
        public const string MainNetHelius = "https://mainnet.helius-rpc.com/?api-key=3109fef3-9cd2-482a-ba34-f050a9d82e9e";
        public const string MainNet = "https://api.mainnet-beta.solana.com";
    }
}