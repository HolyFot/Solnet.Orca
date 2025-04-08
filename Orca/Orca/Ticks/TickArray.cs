using Orca.Accounts;
using Solnet.Wallet;


namespace Orca.Ticks
{
    /// <summary>
    /// Encapsulates a tick array with its address. 
    /// </summary>
    public class TickArrayContainer
    {
        public PublicKey Address { get; set; }
        public TickArray Data { get; set; }
    }
}
