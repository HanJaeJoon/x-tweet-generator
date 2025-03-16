using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TweetGenerator.Models;
using TweetGenerator.Services;

namespace TweetGenerator;

public class Function(Config config, ILoggerFactory loggerFactory, OpenAIService openAiService, TweetService tweetService, SlackService slackService)
{
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

        var stockInfo = await YahooFinanceService.GetPriceInfo([.. config.Stocks.Select(s => s.Symbol)]);

        foreach (var stock in config.Stocks)
        {
            _logger.LogInformation("[Stock: {stock}]", stock);

            var symbol = stock.Symbol;
            var security = stockInfo[symbol];

            _logger.LogDebug($"validate");

            if (security.MarketState is "REGULAR")
            {
                _logger.LogInformation("Market is open");
                return;
            }

            var marketTime = DateTimeOffset.FromUnixTimeSeconds(security.RegularMarketTime).UtcDateTime;
            var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var estNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, estZone);
            var estMarketTime = TimeZoneInfo.ConvertTimeFromUtc(marketTime, estZone);

            if (estMarketTime.Day != estNow.Day)
            {
                _logger.LogInformation("Invalid Day");
                return;
            }

            _logger.LogInformation($"create image using DALL-E 3 API");

            var prompt = openAiService.GetPrompt(security.RegularMarketChange > 0)
                .Replace("[stockName]", security.ShortName)
                .Replace("[currentPrice]", string.Format("{0:N2}", security.RegularMarketPrice))
                .Replace("[priceChange]", string.Format("{0:N2}", security.RegularMarketChange))
            ;

            var imageByte = await openAiService.CreateImage(prompt);

            var content = $"""
            [{estMarketTime:yyyy-MM-dd}]
            the stock price of ${symbol}
            ${string.Format("{0:N2}", security.RegularMarketPrice)}
            {(security.RegularMarketChange > 0 ? "+" : "")}{string.Format("{0:N2}", security.RegularMarketChange)} ({string.Format("{0:N2}", security.RegularMarketChangePercent)}%)
            """;

            _logger.LogInformation("post tweet using X API");
            await tweetService.PostTweet(content, imageByte);

            _logger.LogInformation("send slack message using Slack API");
            await slackService.SendMessage(stock.SlackChannel, symbol, content, imageByte);
        }
    }
}
