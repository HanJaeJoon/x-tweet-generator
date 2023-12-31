﻿using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TweetGenerator.Services;

namespace TweetGenerator;

public class Function(ILoggerFactory loggerFactory, YahooFinanceService yahooFinanceService, OpenAiService openAiService, TweetService tweetService)
{
    private const string _symbol = "TSLA";

    private readonly ILogger _logger = loggerFactory.CreateLogger<Function>();

    [Function("GenerateTweet")]
    public async Task GenerateTweet(
#if DEBUG
        [TimerTrigger("30 0 16 * * Mon-Fri", RunOnStartup = true)] TimerInfo myTimer
#else
        [TimerTrigger("30 0 16 * * Mon-Fri")] TimerInfo myTimer
#endif
        )
    {
        _logger.LogDebug($"get stock price using Yahoo Finance API");
        var (marketState, stockName, marketTime, price, change, changePercent) = await yahooFinanceService.GetPriceInfo(_symbol);

        _logger.LogDebug($"validate");
        if (marketState is "OPEN")
        {
            _logger.LogInformation($"Market is open");
            return;
        }

        TimeZoneInfo estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

        DateTime estNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, estZone);
        DateTime estMarketTime = TimeZoneInfo.ConvertTimeFromUtc(marketTime, estZone);

        if (estMarketTime.Day != estNow.Day)
        {
            _logger.LogInformation($"Invalid Day");
            return;
        }

        byte[] imageByte = [];

        _logger.LogInformation($"create image using DALL-E 3 API");
        var prompt = openAiService.GetPrompt(change > 0)
            .Replace("[stockName]", stockName)
            .Replace("[currentPrice]", string.Format("{0:N2}", price))
            .Replace("[priceChange]", string.Format("{0:N2}", change))
            ;

        // JJ: 일단 크레딧 아끼자
        //imageByte = await openAiService.CreateImage(prompt);

        _logger.LogInformation($"post tweet using X API");
        string content = $"""
        {estMarketTime:yyyy-MM-dd} {stockName} stock price is ${string.Format("{0:N2}", price)}
        {(change > 0 ? "+" : "")}{string.Format("{0:N2}", change)} ({string.Format("{0:N2}", changePercent)}%)
        """;

        await tweetService.PostTweet(content, imageByte);
    }
}