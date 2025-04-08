Solnet.Orca SDK:

Converted the Unity.Solana.Orca (by magicblocks-labs) package to Solnet.

EXAMPLE CODE:

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
            SolHelper.ConvertToLamports(1.5m), //amount
            slippage, //slippage
            orcaAPI.ctx.ProgramId //program ID
        );
        Console.WriteLine($"Quote Out Amount: {quote.EstimatedAmountOut}, direction: {quote.AtoB.ToString()}");

        //Swap With Quote
        /*Solnet.Rpc.Models.Transaction transResult = await orcaAPI.dex.SwapWithQuote(
            pool2.Address,
            quote,
            false,
            Solnet.Rpc.Types.Commitment.Finalized); //AtoB Direction
        */
    
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
    
        SwapParams paramz = orcaAPI.GenerateSwapParams(
            orcaAPI.ctx,
            tokenA,      //TokenA
            tokenB,      //TokenB
            pool3.TokenVaultA,
            pool3.TokenVaultB,
            pool3.Address, //Pool Address
            tickArray, 
            oraclePda1.PublicKey, //Oracle Address
            (BigInteger)amount, //Amount
            (BigInteger)amount, //Amount Threshold
            0, //Sqrt Price Limit
            true, //Amount Specified is Input
            true //AtoB Direction
        );

        //Build Transaction Instructions
        TransactionInstruction instr = await orcaAPI.GetSwapInstructions(orcaAPI.ctx, paramz);
        Console.WriteLine($"Got Fast Quote Trans.");
    
        //Swap Using Parameters
        //RequestResult<string> swapResult = await orcaAPI.SwapAsync(orcaAPI.ctx, paramz);
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
