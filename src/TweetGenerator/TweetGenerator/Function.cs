using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TweetGenerator.Services;

namespace TweetGenerator;

public class Function(ILoggerFactory loggerFactory, YahooFinanceService yahooFinanceService, OpenAiService openAiService, TweetService tweetService)
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
        _logger.LogInformation($"Get Telsa price using Yahoo Finance API");
        var (price, change, changePercent) = await yahooFinanceService.GetPriceInfo();

        byte[] imageByte = [];

        // JJ: 일단 크레딧 아끼자
#pragma warning disable CS0162 // Unreachable code detected
        if (false)
        {
            _logger.LogInformation($"create image using DALL-E 3 API");
            imageByte = await openAiService.CreateImage();
        }

        _logger.LogInformation($"post tweet using X API");
        string content = $"""
            [API 테스트]
            현재 테슬라 주가는 {string.Format("{0:N2}", price)}$ 입니다.
            어제보다 {string.Format("{0:N2}", change)} ({string.Format("{0:N2}", changePercent)}%) {(change > 0 ? "상승" : "하락")}했습니다.
        """;
        _logger.LogInformation(content);
        //await tweetService.PostTweet(content, imageByte);
    }
}
