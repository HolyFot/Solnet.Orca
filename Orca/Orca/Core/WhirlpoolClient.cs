using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Threading.Tasks;
using Solnet.Programs.Abstract;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Core.Sockets;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using Solnet.Rpc.Models;
using Solnet.Rpc.Builders;
using Orca.Program;
using Orca.Errors;
using Orca.Accounts;
using Orca.Types;


namespace Orca
{
    //TODO: (MID) this is partial class; add helper methods to make it easier to call these methods 
    public partial class WhirlpoolClient : TransactionalBaseClient<WhirlpoolErrorType>
    {
        public Commitment DefaultCommitment = Commitment.Finalized;
        
        public WhirlpoolClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId) : base(rpcClient, streamingRpcClient, programId)
        {
        }

        public async Task<Solnet.Programs.Models.ProgramAccountsResultWrapper<List<WhirlpoolsConfig>>> GetWhirlpoolsConfigsAsync(string programAddress, Commitment? commitment = null)
        {
            if (commitment == null) 
                commitment = DefaultCommitment;
                
            var list = new List<Solnet.Rpc.Models.MemCmp>{new() {Bytes = WhirlpoolsConfig.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment.Value, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<WhirlpoolsConfig>>(res);
            List<WhirlpoolsConfig> resultingAccounts = new(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => WhirlpoolsConfig.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<WhirlpoolsConfig>>(res, resultingAccounts);
        }

        public async Task<Solnet.Programs.Models.ProgramAccountsResultWrapper<List<FeeTier>>> GetFeeTiersAsync(string programAddress, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            var list = new List<Solnet.Rpc.Models.MemCmp>{new() {Bytes = FeeTier.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment.Value, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<FeeTier>>(res);
            List<FeeTier> resultingAccounts = new List<FeeTier>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => FeeTier.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<FeeTier>>(res, resultingAccounts);
        }

        public async Task<Solnet.Programs.Models.ProgramAccountsResultWrapper<List<Position>>> GetPositionsAsync(string programAddress, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            var list = new List<Solnet.Rpc.Models.MemCmp>{new Solnet.Rpc.Models.MemCmp{Bytes = Position.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment.Value, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<Position>>(res);
            List<Position> resultingAccounts = new List<Position>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Position.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<Position>>(res, resultingAccounts);
        }

        public async Task<Solnet.Programs.Models.ProgramAccountsResultWrapper<List<TickArray>>> GetTickArraysAsync(string programAddress, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            var list = new List<Solnet.Rpc.Models.MemCmp>{new() {Bytes = TickArray.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment.Value, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<TickArray>>(res);
            List<TickArray> resultingAccounts = new List<TickArray>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => TickArray.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<TickArray>>(res, resultingAccounts);
        }

        public async Task<Solnet.Programs.Models.ProgramAccountsResultWrapper<List<Whirlpool>>> GetWhirlpoolsAsync(string programAddress, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            var list = new List<Solnet.Rpc.Models.MemCmp>{new() {Bytes = Whirlpool.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment.Value, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<Whirlpool>>(res);
            List<Whirlpool> resultingAccounts = new List<Whirlpool>(res.Result.Count);
            return new Solnet.Programs.Models.ProgramAccountsResultWrapper<List<Whirlpool>>(res, resultingAccounts);
        }

        public async Task<Solnet.Programs.Models.AccountResultWrapper<WhirlpoolsConfig>> GetWhirlpoolsConfigAsync(
            string accountAddress, 
            Commitment? commitment = null
        )
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment.Value);
            if (!res.WasSuccessful)
                return new Solnet.Programs.Models.AccountResultWrapper<WhirlpoolsConfig>(res);
            var resultingAccount = WhirlpoolsConfig.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solnet.Programs.Models.AccountResultWrapper<WhirlpoolsConfig>(res, resultingAccount);
        }

        public async Task<Solnet.Programs.Models.AccountResultWrapper<FeeTier>> GetFeeTierAsync(string accountAddress, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment.Value);
            if (!res.WasSuccessful)
                return new Solnet.Programs.Models.AccountResultWrapper<FeeTier>(res);
            var resultingAccount = FeeTier.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solnet.Programs.Models.AccountResultWrapper<FeeTier>(res, resultingAccount);
        }

        public async Task<Solnet.Programs.Models.AccountResultWrapper<Position>> GetPositionAsync(string accountAddress, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment.Value);
            if (!res.WasSuccessful)
                return new Solnet.Programs.Models.AccountResultWrapper<Position>(res);
            
            Position resultingAccount = null;

            if (res.Result != null && res?.Result?.Value?.Data != null && res.Result.Value.Data.Count > 0)
            {
                resultingAccount = Position.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            }
            return new Solnet.Programs.Models.AccountResultWrapper<Position>(res, resultingAccount);
        }

        public async Task<Solnet.Programs.Models.AccountResultWrapper<TickArray>> GetTickArrayAsync(string accountAddress, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment.Value);
            if (!res.WasSuccessful)
            {
                Console.WriteLine($"Returned bad tickarray: {res.Result.Value}");
                return new Solnet.Programs.Models.AccountResultWrapper<TickArray>(res);
            }

            TickArray resultingAccount = null; 
            
            if (res.Result != null && res?.Result?.Value?.Data != null && res.Result.Value.Data.Count > 0)
            {
                resultingAccount = TickArray.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
                //Console.WriteLine($"Got Tick for array: start: {resultingAccount.StartTickIndex}, feeGrowth: {resultingAccount.Ticks[0].FeeGrowthOutsideA}");
            }
            else
            {
                Console.WriteLine($"Couldnt get tick data: {res.Result.Value}");
            }
            return new Solnet.Programs.Models.AccountResultWrapper<TickArray>(res, resultingAccount);
        }

        public async Task<Solnet.Programs.Models.AccountResultWrapper<Whirlpool>> GetWhirlpoolAsync(string accountAddress, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment.Value);
            if (!res.WasSuccessful)
                return new Solnet.Programs.Models.AccountResultWrapper<Whirlpool>(res);

            Whirlpool resultingAccount = null;

            if (res.Result != null && res?.Result?.Value?.Data != null && res.Result.Value.Data.Count > 0)
            {
                resultingAccount = Whirlpool.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
                resultingAccount.Address = new PublicKey(accountAddress);
            }
            return new Solnet.Programs.Models.AccountResultWrapper<Whirlpool>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeWhirlpoolsConfigAsync(string accountAddress, System.Action<SubscriptionState, Solnet.Rpc.Messages.ResponseValue<Solnet.Rpc.Models.AccountInfo>, WhirlpoolsConfig> callback, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                WhirlpoolsConfig parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = WhirlpoolsConfig.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment.Value);
            return res;
        }

        public async Task<SubscriptionState> SubscribeFeeTierAsync(string accountAddress, Action<SubscriptionState, Solnet.Rpc.Messages.ResponseValue<Solnet.Rpc.Models.AccountInfo>, FeeTier> callback, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                FeeTier parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = FeeTier.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment.Value);
            return res;
        }

        public async Task<SubscriptionState> SubscribePositionAsync(string accountAddress, Action<SubscriptionState, Solnet.Rpc.Messages.ResponseValue<Solnet.Rpc.Models.AccountInfo>, Position> callback, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Position parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Position.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment.Value);
            return res;
        }

        public async Task<SubscriptionState> SubscribeTickArrayAsync(string accountAddress, Action<SubscriptionState, Solnet.Rpc.Messages.ResponseValue<Solnet.Rpc.Models.AccountInfo>, TickArray> callback, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                TickArray parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = TickArray.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment.Value);
            return res;
        }

        public async Task<SubscriptionState> SubscribeWhirlpoolAsync(string accountAddress, Action<SubscriptionState, Solnet.Rpc.Messages.ResponseValue<Solnet.Rpc.Models.AccountInfo>, Whirlpool> callback, Commitment? commitment = null)
        {
            if (commitment == null)
                commitment = DefaultCommitment;

            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Whirlpool parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Whirlpool.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment.Value);
            return res;
        }

        public async Task<RequestResult<string>> SendInitializeConfigAsync(
            InitializeConfigAccounts accounts, 
            PublicKey feeAuthority, 
            PublicKey collectProtocolFeesAuthority,
            PublicKey rewardEmissionsSuperAuthority, 
            ushort defaultProtocolFeeRate, 
            PublicKey feePayer, 
            Func<byte[], PublicKey, byte[]> signingCallback, 
            PublicKey programId
        )
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.InitializeConfig(accounts, feeAuthority, collectProtocolFeesAuthority, rewardEmissionsSuperAuthority, defaultProtocolFeeRate, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendInitializePoolAsync(InitializePoolAccounts accounts, WhirlpoolBumps bumps, ushort tickSpacing, BigInteger initialSqrtPrice, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.InitializePool(accounts, bumps, tickSpacing, initialSqrtPrice, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendInitializeTickArrayAsync(InitializeTickArrayAccounts accounts, int startTickIndex, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.InitializeTickArray(accounts, startTickIndex, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendInitializeFeeTierAsync(InitializeFeeTierAccounts accounts, ushort tickSpacing, ushort defaultFeeRate, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.InitializeFeeTier(accounts, tickSpacing, defaultFeeRate, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendInitializeRewardAsync(InitializeRewardAccounts accounts, byte rewardIndex, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.InitializeReward(accounts, rewardIndex, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetRewardEmissionsAsync(SetRewardEmissionsAccounts accounts, byte rewardIndex, BigInteger emissionsPerSecondX64, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetRewardEmissions(accounts, rewardIndex, emissionsPerSecondX64, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendOpenPositionAsync(OpenPositionAccounts accounts, OpenPositionBumps bumps, int tickLowerIndex, int tickUpperIndex, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.OpenPosition(accounts, bumps, tickLowerIndex, tickUpperIndex, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendOpenPositionWithMetadataAsync(OpenPositionWithMetadataAccounts accounts, OpenPositionWithMetadataBumps bumps, int tickLowerIndex, int tickUpperIndex, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.OpenPositionWithMetadata(accounts, bumps, tickLowerIndex, tickUpperIndex, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendIncreaseLiquidityAsync(IncreaseLiquidityAccounts accounts, BigInteger liquidityAmount, ulong tokenMaxA, ulong tokenMaxB, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.IncreaseLiquidity(accounts, liquidityAmount, tokenMaxA, tokenMaxB, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendDecreaseLiquidityAsync(DecreaseLiquidityAccounts accounts, BigInteger liquidityAmount, ulong tokenMinA, ulong tokenMinB, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.DecreaseLiquidity(accounts, liquidityAmount, tokenMinA, tokenMinB, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendUpdateFeesAndRewardsAsync(UpdateFeesAndRewardsAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.UpdateFeesAndRewards(accounts, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendCollectFeesAsync(CollectFeesAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.CollectFees(accounts, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendCollectRewardAsync(CollectRewardAccounts accounts, byte rewardIndex, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.CollectReward(accounts, rewardIndex, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendCollectProtocolFeesAsync(CollectProtocolFeesAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.CollectProtocolFees(accounts, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSwapAsync(SwapAccounts accounts, ulong amount, ulong otherAmountThreshold, BigInteger sqrtPriceLimit, bool amountSpecifiedIsInput, bool aToB, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.Swap(accounts, amount, otherAmountThreshold, sqrtPriceLimit, amountSpecifiedIsInput, aToB, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<TransactionInstruction> GetSwapInstructionsAsync(SwapAccounts accounts, ulong amount, ulong otherAmountThreshold, BigInteger sqrtPriceLimit, bool amountSpecifiedIsInput, bool aToB, PublicKey feePayer, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.Swap(accounts, amount, otherAmountThreshold, sqrtPriceLimit, amountSpecifiedIsInput, aToB, programId);
            return instr;
        }

        /// <summary>
        /// Sends a SwapV2 transaction.
        /// </summary>
        /// <param name="accounts">The accounts for the SwapV2 instruction.</param>
        /// <param name="amount">The amount to swap.</param>
        /// <param name="otherAmountThreshold">The other amount threshold.</param>
        /// <param name="sqrtPriceLimit">The sqrt price limit.</param>
        /// <param name="amountSpecifiedIsInput">Whether the amount specified is input.</param>
        /// <param name="aToB">Whether the swap is from A to B.</param>
        /// <param name="feePayer">The fee payer for the transaction.</param>
        /// <param name="signingCallback">The callback for signing the transaction.</param>
        /// <param name="programId">The program ID.</param>
        /// <param name="supplementalTickArrays">Optional list of supplemental tick arrays.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the request result.</returns>
        public async Task<RequestResult<string>> SendSwapV2Async(SwapV2Accounts accounts, ulong amount, ulong otherAmountThreshold, BigInteger sqrtPriceLimit, bool amountSpecifiedIsInput, bool aToB, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId, List<AccountMeta> supplementalTickArrays = null)
        {
            TransactionInstruction instr = WhirlpoolProgram.SwapV2(
                accounts, amount, otherAmountThreshold,
                sqrtPriceLimit, amountSpecifiedIsInput,
                aToB, programId, supplementalTickArrays
            );
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        /// <summary>
        /// Gets Instructions for a SwapV2 transaction.
        /// </summary>
        /// <param name="accounts">The accounts for the SwapV2 instruction.</param>
        /// <param name="amount">The amount to swap.</param>
        /// <param name="otherAmountThreshold">The other amount threshold.</param>
        /// <param name="sqrtPriceLimit">The sqrt price limit.</param>
        /// <param name="amountSpecifiedIsInput">Whether the amount specified is input.</param>
        /// <param name="aToB">Whether the swap is from A to B.</param>
        /// <param name="feePayer">The fee payer for the transaction.</param>
        /// <param name="signingCallback">The callback for signing the transaction.</param>
        /// <param name="programId">The program ID.</param>
        /// <param name="supplementalTickArrays">Optional list of supplemental tick arrays.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the request result.</returns>
        public async Task<TransactionInstruction> GetSwapV2InstructionsAsync(SwapV2Accounts accounts,ulong amount,ulong otherAmountThreshold,BigInteger sqrtPriceLimit,bool amountSpecifiedIsInput,bool aToB,PublicKey programId,List<AccountMeta> supplementalTickArrays = null)
        {
            await Task.Yield(); // Ensure it's treated as an async method by the compiler.
            return WhirlpoolProgram.SwapV2(
                accounts,amount,otherAmountThreshold,
                sqrtPriceLimit,amountSpecifiedIsInput,
                aToB,programId,supplementalTickArrays
            );
        }

        /// <summary>
        /// Sends a transaction to close a position with Token-2022 extensions.
        /// </summary>
        /// <param name="accounts">The accounts for the ClosePositionV2 instruction.</param>
        /// <param name="feePayer">The fee payer for the transaction.</param>
        /// <param name="signingCallback">The callback for signing the transaction.</param>
        /// <param name="programId">The program ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the request result.</returns>
        public async Task<RequestResult<string>> SendClosePositionV2(
            ClosePositionWithTokenExtensionsAccounts accounts,PublicKey feePayer,Func<byte[], PublicKey, byte[]> signingCallback,PublicKey programId)
        {
            TransactionInstruction instr = WhirlpoolProgram.ClosePositionV2(accounts, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        /// <summary>
        /// Sends a transaction to open a position with Token-2022 extensions.
        /// </summary>
        /// <param name="accounts">The accounts for the OpenPositionV2 instruction.</param>
        /// <param name="bumps">The bumps for PDAs related to the position and its metadata.</param>
        /// <param name="tickLowerIndex">The lower tick index for the position.</param>
        /// <param name="tickUpperIndex">The upper tick index for the position.</param>
        /// <param name="feePayer">The fee payer for the transaction.</param>
        /// <param name="signingCallback">The callback for signing the transaction.</param>
        /// <param name="programId">The program ID.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the request result.</returns>
        public async Task<RequestResult<string>> SendOpenPositionV2(
            OpenPositionWithTokenExtensionsAccounts accounts,OpenPositionWithMetadataBumps bumps,int tickLowerIndex,
            int tickUpperIndex,PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback,PublicKey programId)
        {
            TransactionInstruction instr = WhirlpoolProgram.OpenPositionV2(
                accounts,
                bumps,
                tickLowerIndex,
                tickUpperIndex,
                programId
            );
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendClosePositionAsync(ClosePositionAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.ClosePosition(accounts, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetDefaultFeeRateAsync(SetDefaultFeeRateAccounts accounts, ushort defaultFeeRate, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetDefaultFeeRate(accounts, defaultFeeRate, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetDefaultProtocolFeeRateAsync(SetDefaultProtocolFeeRateAccounts accounts, ushort defaultProtocolFeeRate, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetDefaultProtocolFeeRate(accounts, defaultProtocolFeeRate, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetFeeRateAsync(SetFeeRateAccounts accounts, ushort feeRate, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetFeeRate(accounts, feeRate, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetProtocolFeeRateAsync(SetProtocolFeeRateAccounts accounts, ushort protocolFeeRate, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetProtocolFeeRate(accounts, protocolFeeRate, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetFeeAuthorityAsync(SetFeeAuthorityAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetFeeAuthority(accounts, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetCollectProtocolFeesAuthorityAsync(SetCollectProtocolFeesAuthorityAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetCollectProtocolFeesAuthority(accounts, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetRewardAuthorityAsync(SetRewardAuthorityAccounts accounts, byte rewardIndex, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetRewardAuthority(accounts, rewardIndex, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetRewardAuthorityBySuperAuthorityAsync(SetRewardAuthorityBySuperAuthorityAccounts accounts, byte rewardIndex, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetRewardAuthorityBySuperAuthority(accounts, rewardIndex, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        public async Task<RequestResult<string>> SendSetRewardEmissionsSuperAuthorityAsync(SetRewardEmissionsSuperAuthorityAccounts accounts, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, PublicKey programId)
        {
            Solnet.Rpc.Models.TransactionInstruction instr = WhirlpoolProgram.SetRewardEmissionsSuperAuthority(accounts, programId);
            return await SignAndSendTransaction2(instr, feePayer, signingCallback, commitment: DefaultCommitment);
        }

        protected async Task<RequestResult<string>> SignAndSendTransaction2(TransactionInstruction instruction, PublicKey feePayer, Func<byte[], PublicKey, byte[]> signingCallback, Commitment commitment = Commitment.Finalized)
        {
            TransactionBuilder tb = new TransactionBuilder();
            tb.AddInstruction(instruction);
            tb.SetRecentBlockHash((await base.RpcClient.GetLatestBlockHashAsync()).Result.Value.Blockhash);
            tb.SetFeePayer(feePayer);

            byte[] array = tb.CompileMessage(); //Grab Signers
            Message message = Message.Deserialize(array);
            for (int i = 0; i < message.Header.RequiredSignatures; i++)
            {
                tb.AddSignature(signingCallback(array, message.AccountKeys[i])); //Uses Account Signer using Callback
            }
            var transaction = tb.Serialize();

            return await base.RpcClient.SendTransactionAsync(transaction, skipPreflight: false, commitment); //tb.Serialize()
        }

        protected override Dictionary<uint, ProgramError<WhirlpoolErrorType>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<WhirlpoolErrorType>>{
                {6000U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidEnum, "Enum value could not be converted")},
                {6001U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidStartTick, "Invalid start tick index provided.")},
                {6002U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.TickArrayExistInPool, "Tick-array already exists in this whirlpool")},
                {6003U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.TickArrayIndexOutofBounds, "Attempt to search for a tick-array failed")}, 
                {6004U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidTickSpacing, "Tick-spacing is not supported")}, 
                {6005U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.ClosePositionNotEmpty, "Position is not empty It cannot be closed")}, 
                {6006U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.DivideByZero, "Unable to divide by zero")}, 
                {6007U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.NumberCastError, "Unable to cast number into BigInt")}, 
                {6008U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.NumberDownCastError, "Unable to down cast number")}, 
                {6009U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.TickNotFound, "Tick not found within tick array")}, 
                {6010U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidTickIndex, "Provided tick index is either out of bounds or uninitializable")}, 
                {6011U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.SqrtPriceOutOfBounds, "Provided sqrt price out of bounds")}, 
                {6012U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.LiquidityZero, "Liquidity amount must be greater than zero")}, 
                {6013U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.LiquidityTooHigh, "Liquidity amount must be less than i64::MAX")}, 
                {6014U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.LiquidityOverflow, "Liquidity overflow")}, 
                {6015U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.LiquidityUnderflow, "Liquidity underflow")}, 
                {6016U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.LiquidityNetError, "Tick liquidity net underflowed or overflowed")}, 
                {6017U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.TokenMaxExceeded, "Exceeded token max")}, 
                {6018U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.TokenMinSubceeded, "Did not meet token min")},
                {6019U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.MissingOrInvalidDelegate, "Position token account has a missing or invalid delegate")}, 
                {6020U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidPositionTokenAmount, "Position token amount must be 1")}, 
                {6021U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidTimestampConversion, "Timestamp should be convertible from i64 to u64")}, 
                {6022U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidTimestamp, "Timestamp should be greater than the last updated timestamp")}, 
                {6023U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidTickArraySequence, "Invalid tick array sequence provided for instruction.")}, 
                {6024U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidTokenMintOrder, "Token Mint in wrong order")}, {6025U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.RewardNotInitialized, "Reward not initialized")}, {6026U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidRewardIndex, "Invalid reward index")}, {6027U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.RewardVaultAmountInsufficient, "Reward vault requires amount to support emissions for at least one day")}, {6028U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.FeeRateMaxExceeded, "Exceeded max fee rate")}, {6029U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.ProtocolFeeRateMaxExceeded, "Exceeded max protocol fee rate")}, {6030U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.MultiplicationShiftRightOverflow, "Multiplication with shift right overflow")}, {6031U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.MulDivOverflow, "Muldiv overflow")}, {6032U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.MulDivInvalidInput, "Invalid div_u256 input")}, {6033U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.MultiplicationOverflow, "Multiplication overflow")}, {6034U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.InvalidSqrtPriceLimitDirection, "Provided SqrtPriceLimit not in the same direction as the swap.")}, {6035U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.ZeroTradableAmount, "There are no tradable amount to swap.")}, {6036U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.AmountOutBelowMinimum, "Amount out below minimum threshold")}, 
                {6037U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.AmountInAboveMaximum, "Amount in above maximum threshold")}, 
                {6038U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.TickArraySequenceInvalidIndex, "Invalid index for tick array sequence")}, 
                {6039U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.AmountCalcOverflow, "Amount calculated overflows")}, 
                {6040U, new ProgramError<WhirlpoolErrorType>(WhirlpoolErrorType.AmountRemainingOverflow, "Amount remaining overflows")},
                };
        }
    }
}