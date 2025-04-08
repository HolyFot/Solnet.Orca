using System;
using System.Numerics;
using System.Collections.Generic;

using Solnet.Programs.Utilities;
using Solnet.Wallet;

using Orca.Types;

namespace Orca.Program
{
    public static class WhirlpoolProgram
    {
        public static Solnet.Rpc.Models.TransactionInstruction InitializeConfig(
            InitializeConfigAccounts accounts, PublicKey feeAuthority, PublicKey collectProtocolFeesAuthority,
            PublicKey rewardEmissionsSuperAuthority, ushort defaultProtocolFeeRate, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Config, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Funder, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(5099410418541363152UL, offset);
            offset += 8;
            data.WritePubKey(feeAuthority, offset);
            offset += 32;
            data.WritePubKey(collectProtocolFeesAuthority, offset);
            offset += 32;
            data.WritePubKey(rewardEmissionsSuperAuthority, offset);
            offset += 32;
            data.WriteU16(defaultProtocolFeeRate, offset);
            offset += 2;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction InitializePool(InitializePoolAccounts accounts,
            WhirlpoolBumps bumps, ushort tickSpacing, BigInteger initialSqrtPrice, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenMintA, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenMintB, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Funder, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultA, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultB, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.FeeTier, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Rent, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(2947797634800858207UL, offset);
            offset += 8;
            offset += bumps.Serialize(data, offset);
            data.WriteU16(tickSpacing, offset);
            offset += 2;
            data.WriteBigInt(initialSqrtPrice, offset, 16, true);
            offset += 16;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction InitializeTickArray(
            InitializeTickArrayAccounts accounts, int startTickIndex, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Funder, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TickArray, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(13300637739260165131UL, offset);
            offset += 8;
            data.WriteS32(startTickIndex, offset);
            offset += 4;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction InitializeFeeTier(
            InitializeFeeTierAccounts accounts, ushort tickSpacing, ushort defaultFeeRate, PublicKey programId)
        {
            List<Solnet.Rpc.Models.AccountMeta> keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Config, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.FeeTier, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Funder, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.FeeAuthority, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(2173552452913875639UL, offset);
            offset += 8;
            data.WriteU16(tickSpacing, offset);
            offset += 2;
            data.WriteU16(defaultFeeRate, offset);
            offset += 2;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction InitializeReward(
            InitializeRewardAccounts accounts, byte rewardIndex, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.RewardAuthority, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Funder, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.RewardMint, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.RewardVault, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Rent, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(4964798518905571167UL, offset);
            offset += 8;
            data.WriteU8(rewardIndex, offset);
            offset += 1;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetRewardEmissions(
            SetRewardEmissionsAccounts accounts, byte rewardIndex, BigInteger emissionsPerSecondX64,
            PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.RewardAuthority, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.RewardVault, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(17589846754647786765UL, offset);
            offset += 8;
            data.WriteU8(rewardIndex, offset);
            offset += 1;
            data.WriteBigInt(emissionsPerSecondX64, offset, 16, true);
            offset += 16;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction OpenPosition(OpenPositionAccounts accounts,
            OpenPositionBumps bumps, int tickLowerIndex, int tickUpperIndex, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Funder, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Owner, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Position, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.PositionMint, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.PositionTokenAccount, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Rent, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.AssociatedTokenProgram, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(3598543293755916423UL, offset);
            offset += 8;
            offset += bumps.Serialize(data, offset);
            data.WriteS32(tickLowerIndex, offset);
            offset += 4;
            data.WriteS32(tickUpperIndex, offset);
            offset += 4;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction OpenPositionWithMetadata(
            OpenPositionWithMetadataAccounts accounts, OpenPositionWithMetadataBumps bumps, int tickLowerIndex,
            int tickUpperIndex, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Funder, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Owner, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Position, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.PositionMint, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.PositionMetadataAccount, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.PositionTokenAccount, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Rent, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.AssociatedTokenProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.MetadataProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.MetadataUpdateAuth, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(4327517488150879730UL, offset);
            offset += 8;
            offset += bumps.Serialize(data, offset);
            data.WriteS32(tickLowerIndex, offset);
            offset += 4;
            data.WriteS32(tickUpperIndex, offset);
            offset += 4;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction IncreaseLiquidity(
            IncreaseLiquidityAccounts accounts, BigInteger liquidityAmount, ulong tokenMaxA, ulong tokenMaxB,
            PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.PositionAuthority, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Position, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.PositionTokenAccount, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenOwnerAccountA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenOwnerAccountB, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultB, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TickArrayLower, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TickArrayUpper, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(12897127415619492910UL, offset);
            offset += 8;
            data.WriteBigInt(liquidityAmount, offset, 16, true);
            offset += 16;
            data.WriteU64(tokenMaxA, offset);
            offset += 8;
            data.WriteU64(tokenMaxB, offset);
            offset += 8;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction DecreaseLiquidity(
            DecreaseLiquidityAccounts accounts, BigInteger liquidityAmount, ulong tokenMinA, ulong tokenMinB,
            PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.PositionAuthority, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Position, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.PositionTokenAccount, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenOwnerAccountA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenOwnerAccountB, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultB, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TickArrayLower, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TickArrayUpper, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(84542997123835552UL, offset);
            offset += 8;
            data.WriteBigInt(liquidityAmount, offset, 16, true);
            offset += 16;
            data.WriteU64(tokenMinA, offset);
            offset += 8;
            data.WriteU64(tokenMinB, offset);
            offset += 8;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction UpdateFeesAndRewards(
            UpdateFeesAndRewardsAccounts accounts, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Position, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TickArrayLower, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TickArrayUpper, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(16090184905488262810UL, offset);
            offset += 8;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction CollectFees(CollectFeesAccounts accounts,
            PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.PositionAuthority, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Position, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.PositionTokenAccount, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenOwnerAccountA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenOwnerAccountB, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultB, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(13120034779146721444UL, offset);
            offset += 8;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction CollectReward(CollectRewardAccounts accounts,
            byte rewardIndex, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.PositionAuthority, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Position, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.PositionTokenAccount, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.RewardOwnerAccount, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.RewardVault, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(2500038024235320646UL, offset);
            offset += 8;
            data.WriteU8(rewardIndex, offset);
            offset += 1;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction CollectProtocolFees(
            CollectProtocolFeesAccounts accounts, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.CollectProtocolFeesAuthority, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultB, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenDestinationA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenDestinationB, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(15872570295674422038UL, offset);
            offset += 8;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction Swap(SwapAccounts accounts, ulong amount,
            ulong otherAmountThreshold, BigInteger sqrtPriceLimit, bool amountSpecifiedIsInput, bool aToB,
            PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenAuthority, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenOwnerAccountA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultA, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenOwnerAccountB, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TokenVaultB, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TickArray0, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TickArray1, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.TickArray2, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.Oracle, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(14449647541112719096UL, offset);
            offset += 8;
            data.WriteU64(amount, offset);
            offset += 8;
            data.WriteU64(otherAmountThreshold, offset);
            offset += 8;
            data.WriteBigInt(sqrtPriceLimit, offset, 16, true);
            offset += 16;
            data.WriteBool(amountSpecifiedIsInput, offset);
            offset += 1;
            data.WriteBool(aToB, offset);
            offset += 1;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction ClosePosition(ClosePositionAccounts accounts,
            PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.PositionAuthority, true),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Receiver, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Position, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.PositionMint, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.PositionTokenAccount, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.TokenProgram, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(7089303740684011131UL, offset);
            offset += 8;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetDefaultFeeRate(
            SetDefaultFeeRateAccounts accounts, ushort defaultFeeRate, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.FeeTier, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.FeeAuthority, true)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(16487930808298297206UL, offset);
            offset += 8;
            data.WriteU16(defaultFeeRate, offset);
            offset += 2;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetDefaultProtocolFeeRate(
            SetDefaultProtocolFeeRateAccounts accounts, ushort defaultProtocolFeeRate, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.FeeAuthority, true)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(24245983252172139UL, offset);
            offset += 8;
            data.WriteU16(defaultProtocolFeeRate, offset);
            offset += 2;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetFeeRate(SetFeeRateAccounts accounts,
            ushort feeRate, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.FeeAuthority, true)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(476972577635038005UL, offset);
            offset += 8;
            data.WriteU16(feeRate, offset);
            offset += 2;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetProtocolFeeRate(
            SetProtocolFeeRateAccounts accounts, ushort protocolFeeRate, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.FeeAuthority, true)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(9483542439018104671UL, offset);
            offset += 8;
            data.WriteU16(protocolFeeRate, offset);
            offset += 2;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetFeeAuthority(
            SetFeeAuthorityAccounts accounts, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.FeeAuthority, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.NewFeeAuthority, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(9539017555791970591UL, offset);
            offset += 8;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetCollectProtocolFeesAuthority(
            SetCollectProtocolFeesAuthorityAccounts accounts, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.CollectProtocolFeesAuthority, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.NewCollectProtocolFeesAuthority, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(4893690461331232290UL, offset);
            offset += 8;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetRewardAuthority(
            SetRewardAuthorityAccounts accounts, byte rewardIndex, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.RewardAuthority, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.NewRewardAuthority, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(9175270962884978466UL, offset);
            offset += 8;
            data.WriteU8(rewardIndex, offset);
            offset += 1;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetRewardAuthorityBySuperAuthority(
            SetRewardAuthorityBySuperAuthorityAccounts accounts, byte rewardIndex, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.Whirlpool, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.RewardEmissionsSuperAuthority, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.NewRewardAuthority, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(1817305343215639280UL, offset);
            offset += 8;
            data.WriteU8(rewardIndex, offset);
            offset += 1;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }

        public static Solnet.Rpc.Models.TransactionInstruction SetRewardEmissionsSuperAuthority(
            SetRewardEmissionsSuperAuthorityAccounts accounts, PublicKey programId)
        {
            var keys = new List<Solnet.Rpc.Models.AccountMeta>
            {
                Solnet.Rpc.Models.AccountMeta.Writable(accounts.WhirlpoolsConfig, false),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.RewardEmissionsSuperAuthority, true),
                Solnet.Rpc.Models.AccountMeta.ReadOnly(accounts.NewRewardEmissionsSuperAuthority, false)
            };
            byte[] data = new byte[1200];
            int offset = 0;
            data.WriteU64(13209682757187798479UL, offset);
            offset += 8;
            byte[] resultData = new byte[offset];
            Array.Copy(data, resultData, offset);
            return new Solnet.Rpc.Models.TransactionInstruction
            { Keys = keys, ProgramId = programId.KeyBytes, Data = resultData };
        }
    }
}