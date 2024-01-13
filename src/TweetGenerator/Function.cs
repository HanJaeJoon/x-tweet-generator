using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SlackAPI;
using TweetGenerator.Services;

namespace TweetGenerator;

public class Function(IConfiguration configuration, ILoggerFactory loggerFactory,
    YahooFinanceService yahooFinanceService, OpenAiService openAiService, TweetService tweetService)
{
    private readonly string[] _symbols = configuration["symbols"].Split(',');

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
        var result = await yahooFinanceService.GetPriceInfo(_symbols);

        // JJ: multi symbols support
        var (marketState, stockName, marketTime, price, change, changePercent) = result.First();
        var symbol = _symbols.First();

        _logger.LogDebug($"validate");
        if (marketState is "REGULAR")
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

        var (createdImageByte, imageUrl) = await openAiService.CreateImage(prompt);

        imageByte = createdImageByte;

        _logger.LogInformation($"post tweet using X API");
        string content = $"""
        [{estMarketTime:yyyy-MM-dd}]
        the stock price of {GetStockNameForX(symbol)}
        ${string.Format("{0:N2}", price)}
        {(change > 0 ? "+" : "")}{string.Format("{0:N2}", change)} ({string.Format("{0:N2}", changePercent)}%)
        """;

        static string? GetStockNameForX(string symbol)
        {
            return symbol switch
            {
                "TSLA" => "$TSLA",
                _ => $"#{symbol}",
            };
        }

        await tweetService.PostTweet(content, imageByte);

        var slackToken = configuration["SlackToken"];
        var client = new SlackTaskClient(slackToken);
        var channel = configuration["SlackChannel"];
        var attachment = new Attachment()
        {
            image_url = imageUrl,
            fallback = "(image created with DALL-E 3 in Open AI)",
        };

        // JJ: DALL-E 3 최소 용량으로 이미지 생성해도 3MB가 넘어서 슬랙에서 안 보이는 문제
        // html trigger 사용 하는 방법?
        var response = await client.PostMessageAsync(channel, content, attachments: [attachment]);

        if (!response.ok)
        {
            _logger.LogError($"Message sending failed. error: {response.error}");
        }
    }
}
