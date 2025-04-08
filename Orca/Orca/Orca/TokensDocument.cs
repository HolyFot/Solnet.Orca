using Orca.Models;
using System.Collections.Generic;

namespace Orca;

/// <summary>
/// Serialized from JSON; represents the entire JSON output. 
/// </summary>
public class TokensDocument
{
    public List<TokenData> tokens; 
}