using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TweetGenerator.Services;

namespace TweetGenerator;

public class Function(IConfiguration configuration, ILoggerFactory loggerFactory, OpenAIService openAiService, TweetService tweetService, SlackService slackService)
{
    private readonly string[] _symbols = configuration["symbols"]?.Split(',') ?? [];
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
        _logger.LogDebug("get stock price using Yahoo Finance API at {time}", myTimer.ScheduleStatus?.Last ?? DateTime.UtcNow);
        var result = await YahooFinanceService.GetPriceInfo(_symbols);

        // JJ: multi symbols support
        var (marketState, stockName, marketTime, price, change, changePercent) = result.First();
        var symbol = _symbols.First();

        _logger.LogDebug($"validate");
        if (marketState is "REGULAR")
        {
            _logger.LogInformation($"Market is open");
            return;
        }

        var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
        var estNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, estZone);
        var estMarketTime = TimeZoneInfo.ConvertTimeFromUtc(marketTime, estZone);

        if (estMarketTime.Day != estNow.Day)
        {
            _logger.LogInformation($"Invalid Day");
            return;
        }

        _logger.LogInformation($"create image using DALL-E 3 API");

        var prompt = openAiService.GetPrompt(change > 0)
            .Replace("[stockName]", stockName)
            .Replace("[currentPrice]", string.Format("{0:N2}", price))
            .Replace("[priceChange]", string.Format("{0:N2}", change))
            ;

        var imageByte = await openAiService.CreateImage(prompt);

        _logger.LogInformation($"post tweet using X API");

        var content = $"""
        [{estMarketTime:yyyy-MM-dd}]
        the stock price of ${symbol}
        ${string.Format("{0:N2}", price)}
        {(change > 0 ? "+" : "")}{string.Format("{0:N2}", change)} ({string.Format("{0:N2}", changePercent)}%)
        """;

        await tweetService.PostTweet(content, imageByte);
        await slackService.SendMessage(content, imageByte);
    }
}
