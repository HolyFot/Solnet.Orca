using Solnet.Wallet;
using System.Numerics;

namespace Orca.Models;

public class Pool
{
    public PublicKey Address { get; set; }
    
    public PublicKey TokenMintA { get; set; }
    public PublicKey TokenMintB { get; set; }
    
    public BigInteger Liquidity { get; set; }
    public ushort Fee { get; set; }
    public ushort TickSpacing { get; set; }
}