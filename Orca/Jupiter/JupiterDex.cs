using System.Text.Json;
using System.Text.Json.Serialization;
using Orca.Types.Http;
using Orca.Models;
using Orca.Quotes;
//using Solnet.Rpc.Converters;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Solnet.Rpc.Converters;

namespace Orca.Jupiter;

/// <summary>
/// Concrete implementation of IDexAggregator for Jupiter Aggregator. 
/// </summary>
public class JupiterDexAg : IDexAggregator
{
    private readonly PublicKey _account;
    private readonly string _endpoint;
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _serializerOptions;
    private List<TokenData> _tokens;

    /// <summary>
    /// Public constructor; Create the JupiterDexAg instance with the account to use for the aggregator. 
    /// </summary>
    /// <param name="endpoint"></param>
    public JupiterDexAg(string endpoint = "https://quote-api.jup.ag/v6")
    {
        _endpoint = endpoint;
        _httpClient = new HttpClient();
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new EncodingConverter(), //
                new JsonStringEnumConverter()
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Public constructor; Create the JupiterDexAg instance with the account to use for the aggregator. 
    /// </summary>
    /// <param name="account"></param>
    /// <param name="endpoint"></param>
    public JupiterDexAg(PublicKey account, string endpoint = "https://quote-api.jup.ag/v6")
    {
        _account = account;
        _endpoint = endpoint;
        _httpClient = new HttpClient();
        _serializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new Solnet.Rpc.Converters.EncodingConverter(),
                new JsonStringEnumConverter()
            },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <inheritdoc />
    public async Task<SwapQuoteAg> GetSwapQuote(
        PublicKey inputMint,
        PublicKey outputMint,
        BigInteger amount,
        SwapMode swapMode = SwapMode.ExactIn,
        ushort? slippageBps = null,
        List<string> excludeDexes = null,
        bool onlyDirectRoutes = false,
        ushort? platformFeeBps = null,
        ushort? maxAccounts = null)
        {
            // Construct the query parameters
            List<KeyValuePair<string, string>> queryParams = new()
            {
                new("inputMint", inputMint.ToString()),
                new("outputMint", outputMint.ToString()),
                new("amount", amount.ToString()),
                new("swapMode", swapMode.ToString()),
                new("asLegacyTransaction", "false")
            };

            if (slippageBps.HasValue) queryParams.Add(new KeyValuePair<string, string>("slippageBps", slippageBps.Value.ToString()));

            if (excludeDexes is { Count: > 0 }) queryParams.AddRange(excludeDexes.Select(dex => new KeyValuePair<string, string>("excludeDexes", dex)));

            queryParams.Add(new KeyValuePair<string, string>("onlyDirectRoutes", onlyDirectRoutes.ToString().ToLower()));

            if (platformFeeBps.HasValue) queryParams.Add(new KeyValuePair<string, string>("platformFeeBps", platformFeeBps.Value.ToString()));

            if (maxAccounts.HasValue) queryParams.Add(new KeyValuePair<string, string>("maxAccounts", maxAccounts.Value.ToString()));

            var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"));

        // Construct the request URL
        var apiUrl = _endpoint + "/quote?" + queryString;

        using var httpReq = new HttpRequestMessage(HttpMethod.Get, apiUrl);

        // execute the REST request
        using var response = await _httpClient.SendAsync(httpReq);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            // Deserialize the response JSON into SwapQuoteAg object
            var swapQuote = JsonSerializer.Deserialize<SwapQuoteAg>(responseBody, _serializerOptions);
            return swapQuote;
        }

        // Handle error scenarios
        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    /// <inheritdoc />
    public async Task<Transaction> Swap(
        SwapQuoteAg quoteResponse,
        PublicKey userPublicKey = null,
        PublicKey destinationTokenAccount = null,
        bool wrapAndUnwrapSol = true,
        bool useSharedAccounts = true,
        PublicKey feeAccount = null,
        BigInteger? computeUnitPriceMicroLamports = null,
        bool useTokenLedger = false)
    {
        userPublicKey ??= _account;

        // Construct the request URL
        var apiUrl = _endpoint + "/swap";

        var req = new SwapRequest()
        {
            QuoteResponse = quoteResponse,
            UserPublicKey = userPublicKey,
            DestinationTokenAccount = destinationTokenAccount,
            WrapAndUnwrapSol = wrapAndUnwrapSol,
            UseSharedAccounts = useSharedAccounts,
            FeeAccount = feeAccount,
            ComputeUnitPriceMicroLamports = computeUnitPriceMicroLamports,
            UseTokenLedger = useTokenLedger,
            AsLegacyTransaction = false
        };

        var requestJson = JsonSerializer.Serialize(req, _serializerOptions);
        var buffer = Encoding.UTF8.GetBytes(requestJson);

        using var httpReq = new HttpRequestMessage(HttpMethod.Post, apiUrl)
        {
            Content = new ByteArrayContent(buffer)
            {
                Headers = {
                    { "Content-Type", "application/json"}
                }
            }
        };

        // execute POST
        using var response = await _httpClient.SendAsync(httpReq);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var res = JsonSerializer.Deserialize<SwapResponse>(responseBody, _serializerOptions);
            return Transaction.Deserialize(res.SwapTransaction);
        }

        // Handle error scenarios
        throw new HttpRequestException($"Request failed with status code: {response.StatusCode}");
    }

    /// <inheritdoc />
    public async Task<IList<TokenData>> GetTokens(TokenListType tokenListType = TokenListType.Strict)
    {
        string url = $"https://token.jup.ag/{tokenListType.ToString().ToLower()}";
        if (_tokens == null)
        {
            //using var client = new HttpClient();
            using var httpReq = new HttpRequestMessage(HttpMethod.Get, url);
            HttpResponseMessage result = await _httpClient.SendAsync(httpReq);
            string response = await result.Content.ReadAsStringAsync();

            // Create a wrapper object for proper deserialization
            string wrappedJson = $"{{\"tokens\": {response} }}";
            using var jsonDoc = JsonDocument.Parse(wrappedJson);
            var tokensElement = jsonDoc.RootElement.GetProperty("tokens");

            var tokensDocument = JsonSerializer.Deserialize<TokensDocument>(
                wrappedJson,
                _serializerOptions
            );

            if (tokensDocument != null)
            {
                _tokens = tokensDocument.tokens.ToList();
            }
        }

        return _tokens;
    }

    /// <inheritdoc />
    public async Task<TokenData> GetTokenBySymbol(string symbol)
    {
        IList<TokenData> tokens = await GetTokens(TokenListType.All);
        return tokens.First(t =>
            string.Equals(t.Symbol, symbol, StringComparison.CurrentCultureIgnoreCase) ||
            string.Equals(t.Symbol, $"${symbol}", StringComparison.CurrentCultureIgnoreCase));
    }

    /// <inheritdoc />
    public async Task<TokenData> GetTokenByMint(string mint)
    {
        IList<TokenData> tokens = await GetTokens(TokenListType.All);
        return tokens.First(t => string.Equals(t.Mint, mint, StringComparison.CurrentCultureIgnoreCase));
    }
}