using Orca.Accounts;
using Orca.Ticks;
using Solnet.Wallet;
using System.Numerics;

namespace Orca.Program
{
    public class InitializeConfigAccounts
    {
        public PublicKey Config { get; set; }

        public PublicKey Funder { get; set; }

        public PublicKey SystemProgram { get; set; }
    }

    public class InitializePoolAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey TokenMintA { get; set; }

        public PublicKey TokenMintB { get; set; }

        public PublicKey Funder { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey TokenVaultA { get; set; }

        public PublicKey TokenVaultB { get; set; }

        public PublicKey FeeTier { get; set; }

        public PublicKey TokenProgram { get; set; }

        public PublicKey SystemProgram { get; set; }

        public PublicKey Rent { get; set; }
    }

    public class InitializeTickArrayAccounts
    {
        public PublicKey Whirlpool { get; set; }

        public PublicKey Funder { get; set; }

        public PublicKey TickArray { get; set; }

        public PublicKey SystemProgram { get; set; }
    }

    public class InitializeFeeTierAccounts
    {
        public PublicKey Config { get; set; }

        public PublicKey FeeTier { get; set; }

        public PublicKey Funder { get; set; }

        public PublicKey FeeAuthority { get; set; }

        public PublicKey SystemProgram { get; set; }
    }

    public class InitializeRewardAccounts
    {
        public PublicKey RewardAuthority { get; set; }

        public PublicKey Funder { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey RewardMint { get; set; }

        public PublicKey RewardVault { get; set; }

        public PublicKey TokenProgram { get; set; }

        public PublicKey SystemProgram { get; set; }

        public PublicKey Rent { get; set; }
    }

    public class SetRewardEmissionsAccounts
    {
        public PublicKey Whirlpool { get; set; }

        public PublicKey RewardAuthority { get; set; }

        public PublicKey RewardVault { get; set; }
    }

    public class IncreaseLiquidityAccounts
    {
        public PublicKey Whirlpool { get; set; }

        public PublicKey TokenProgram { get; set; }

        public PublicKey PositionAuthority { get; set; }

        public PublicKey Position { get; set; }

        public PublicKey PositionTokenAccount { get; set; }

        public PublicKey TokenOwnerAccountA { get; set; }

        public PublicKey TokenOwnerAccountB { get; set; }

        public PublicKey TokenVaultA { get; set; }

        public PublicKey TokenVaultB { get; set; }

        public PublicKey TickArrayLower { get; set; }

        public PublicKey TickArrayUpper { get; set; }
    }

    public class DecreaseLiquidityAccounts
    {
        public PublicKey Whirlpool { get; set; }

        public PublicKey TokenProgram { get; set; }

        public PublicKey PositionAuthority { get; set; }

        public PublicKey Position { get; set; }

        public PublicKey PositionTokenAccount { get; set; }

        public PublicKey TokenOwnerAccountA { get; set; }

        public PublicKey TokenOwnerAccountB { get; set; }

        public PublicKey TokenVaultA { get; set; }

        public PublicKey TokenVaultB { get; set; }

        public PublicKey TickArrayLower { get; set; }

        public PublicKey TickArrayUpper { get; set; }
    }

    public class UpdateFeesAndRewardsAccounts
    {
        public PublicKey Whirlpool { get; set; }

        public PublicKey Position { get; set; }

        public PublicKey TickArrayLower { get; set; }

        public PublicKey TickArrayUpper { get; set; }
    }

    public class CollectFeesAccounts
    {
        public PublicKey Whirlpool { get; set; }

        public PublicKey PositionAuthority { get; set; }

        public PublicKey Position { get; set; }

        public PublicKey PositionTokenAccount { get; set; }

        public PublicKey TokenOwnerAccountA { get; set; }

        public PublicKey TokenVaultA { get; set; }

        public PublicKey TokenOwnerAccountB { get; set; }

        public PublicKey TokenVaultB { get; set; }

        public PublicKey TokenProgram { get; set; }
    }

    public class CollectRewardAccounts
    {
        public PublicKey Whirlpool { get; set; }

        public PublicKey PositionAuthority { get; set; }

        public PublicKey Position { get; set; }

        public PublicKey PositionTokenAccount { get; set; }

        public PublicKey RewardOwnerAccount { get; set; }

        public PublicKey RewardVault { get; set; }

        public PublicKey TokenProgram { get; set; }
    }

    public class CollectProtocolFeesAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey CollectProtocolFeesAuthority { get; set; }

        public PublicKey TokenVaultA { get; set; }

        public PublicKey TokenVaultB { get; set; }

        public PublicKey TokenDestinationA { get; set; }

        public PublicKey TokenDestinationB { get; set; }

        public PublicKey TokenProgram { get; set; }
    }

    public class SwapAccounts
    {
        public PublicKey TokenProgram { get; set; }

        public PublicKey TokenAuthority { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey TokenOwnerAccountA { get; set; }

        public PublicKey TokenVaultA { get; set; }

        public PublicKey TokenOwnerAccountB { get; set; }

        public PublicKey TokenVaultB { get; set; }

        public PublicKey TickArray0 { get; set; }

        public PublicKey TickArray1 { get; set; }

        public PublicKey TickArray2 { get; set; }

        public PublicKey Oracle { get; set; }
    }

    public class SwapParams
    {
        /// <summary>
        /// Accounts for a V1 swap. Populate this if not using V2.
        /// </summary>
        public SwapAccounts Accounts { get; set; }

        /// <summary>
        /// Accounts for a V2 swap. Populate this if using V2.
        /// </summary>
        public SwapV2Accounts AccountsV2 { get; set; }

        public BigInteger Amount { get; set; }
        public BigInteger OtherThresholdAmount { get; set; }
        public BigInteger SqrtPriceLimit { get; set; }
        public bool AmountSpecifiedIsInput { get; set; }
        public bool AtoB { get; set; }
    }

    public class SwapV2Accounts
    {
        public PublicKey TokenProgramA { get; set; }

        public PublicKey TokenProgramB { get; set; }

        public PublicKey MemoProgram { get; set; }

        public PublicKey TokenAuthority { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey TokenMintA { get; set; }

        public PublicKey TokenMintB { get; set; }

        public PublicKey TokenOwnerAccountA { get; set; }

        public PublicKey TokenVaultA { get; set; }

        public PublicKey TokenOwnerAccountB { get; set; }

        public PublicKey TokenVaultB { get; set; }

        public PublicKey TickArray0 { get; set; }

        public PublicKey TickArray1 { get; set; }

        public PublicKey TickArray2 { get; set; }

        public PublicKey Oracle { get; set; }
    }

    public class OpenPositionAccounts
    {
        public PublicKey Funder { get; set; }

        public PublicKey Owner { get; set; }

        public PublicKey Position { get; set; }

        public PublicKey PositionMint { get; set; }

        public PublicKey PositionTokenAccount { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey TokenProgram { get; set; }

        public PublicKey SystemProgram { get; set; }

        public PublicKey Rent { get; set; }

        public PublicKey AssociatedTokenProgram { get; set; }
    }

    public class OpenPositionWithMetadataAccounts
    {
        public PublicKey Funder { get; set; }

        public PublicKey Owner { get; set; }

        public PublicKey Position { get; set; }

        public PublicKey PositionMint { get; set; }

        public PublicKey PositionMetadataAccount { get; set; }

        public PublicKey PositionTokenAccount { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey TokenProgram { get; set; }

        public PublicKey SystemProgram { get; set; }

        public PublicKey Rent { get; set; }

        public PublicKey AssociatedTokenProgram { get; set; }

        public PublicKey MetadataProgram { get; set; }

        public PublicKey MetadataUpdateAuth { get; set; }
    }


    public class ClosePositionAccounts
    {
        public PublicKey PositionAuthority { get; set; }

        public PublicKey Receiver { get; set; }

        public PublicKey Position { get; set; }

        public PublicKey PositionMint { get; set; }

        public PublicKey PositionTokenAccount { get; set; }

        public PublicKey TokenProgram { get; set; }
    }

    /// <summary>
    /// Accounts for closing a position with Token-2022 extensions.
    /// </summary>
    public class ClosePositionWithTokenExtensionsAccounts
    {
        public PublicKey PositionAuthority { get; set; }
        public PublicKey Receiver { get; set; }
        public PublicKey Position { get; set; }
        public PublicKey PositionMint { get; set; }
        public PublicKey PositionTokenAccount { get; set; }
        public PublicKey Token2022Program { get; set; } // Explicitly Token-2022
    }

    /// <summary>
    /// Accounts for opening a position with Token-2022 extensions.
    /// </summary>
    public class OpenPositionWithTokenExtensionsAccounts
    {
        public PublicKey Funder { get; set; }
        public PublicKey Owner { get; set; }
        public PublicKey Position { get; set; }
        public PublicKey PositionMint { get; set; }
        public PublicKey PositionMetadataAccount { get; set; } // For metadata
        public PublicKey PositionTokenAccount { get; set; }
        public PublicKey Whirlpool { get; set; }
        public PublicKey Token2022Program { get; set; } // Explicitly Token-2022
        public PublicKey SystemProgram { get; set; }
        public PublicKey Rent { get; set; }
        public PublicKey AssociatedTokenProgram { get; set; }
        public PublicKey MetadataProgram { get; set; } // For metadata
        public PublicKey MetadataUpdateAuth { get; set; } // For metadata
    }

    public class SetDefaultFeeRateAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey FeeTier { get; set; }

        public PublicKey FeeAuthority { get; set; }
    }

    public class SetDefaultProtocolFeeRateAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey FeeAuthority { get; set; }
    }

    public class SetFeeRateAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey FeeAuthority { get; set; }
    }

    public class SetProtocolFeeRateAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey FeeAuthority { get; set; }
    }

    public class SetFeeAuthorityAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey FeeAuthority { get; set; }

        public PublicKey NewFeeAuthority { get; set; }
    }

    public class SetCollectProtocolFeesAuthorityAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey CollectProtocolFeesAuthority { get; set; }

        public PublicKey NewCollectProtocolFeesAuthority { get; set; }
    }

    public class SetRewardAuthorityAccounts
    {
        public PublicKey Whirlpool { get; set; }

        public PublicKey RewardAuthority { get; set; }

        public PublicKey NewRewardAuthority { get; set; }
    }

    public class SetRewardAuthorityBySuperAuthorityAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey Whirlpool { get; set; }

        public PublicKey RewardEmissionsSuperAuthority { get; set; }

        public PublicKey NewRewardAuthority { get; set; }
    }

    public class SetRewardEmissionsSuperAuthorityAccounts
    {
        public PublicKey WhirlpoolsConfig { get; set; }

        public PublicKey RewardEmissionsSuperAuthority { get; set; }

        public PublicKey NewRewardEmissionsSuperAuthority { get; set; }
    }
}