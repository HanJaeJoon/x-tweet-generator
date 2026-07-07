using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using TweetGenerator.Models;
using TweetGenerator.Services;
using YahooFinanceApi;

namespace TweetGenerator;

public class Function(IConfiguration configuration, ILogger<Function> logger, OpenAIService openAIService, TweetService tweetService, SlackService slackService)
{
    private static readonly TimeZoneInfo _estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

    private readonly ILogger _logger = logger;

    [Function("GenerateTweet")]
    public async Task RunScheduled([TimerTrigger("30 0 16 * * Mon-Fri")] TimerInfo timerInfo)
    {
        var utcNow = DateTime.UtcNow;
        _logger.LogInformation("Timer triggered at: {Now} (Last run: {Last})", utcNow, timerInfo.ScheduleStatus?.Last);

        await RunImpl(utcNow);
    }

    [Function("ManualTrigger")]
    public async Task<HttpResponseData> RunManual([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req)
    {
        if (!DateTimeOffset.TryParse(req.Query["utcTime"], out var utcTime))
        {
            _logger.LogError("Invalid dateTime format");
            return CreateTextResponse(req, HttpStatusCode.BadRequest, "Invalid dateTime format");
        }

        _logger.LogInformation("Manually triggered at: {Now}", utcTime);

        await RunImpl(utcTime.UtcDateTime);

        return CreateTextResponse(req, HttpStatusCode.OK, "Success !!!");
    }

    private static HttpResponseData CreateTextResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString(message);
        return response;
    }

    private async Task RunImpl(DateTime utcNow)
    {
        var stocks = JsonSerializer.Deserialize<List<Stock>>(configuration["Stocks"] ?? "[]") ?? [];
        _logger.LogInformation("Starting tweet generation for {Count} stocks at {Now}", stocks.Count, utcNow);

        var stockInfo = await GetStockInfo(stocks);

        foreach (var stock in stocks)
        {
            var symbol = stock.Symbol;
            _logger.LogInformation("Processing stock: {Symbol}", symbol);

            if (!stockInfo.TryGetValue(symbol, out var security))
            {
                _logger.LogWarning("Stock information not found for symbol: {Symbol}", symbol);
                _logger.LogInformation("stockInfo: {StockInfo}", JsonSerializer.Serialize(stockInfo));
                continue;
            }

            if (security.MarketState is "REGULAR")
            {
                _logger.LogWarning("Market is open (MarketState: {MarketState})", security.MarketState);
                return;
            }

            var marketTime = DateTimeOffset.FromUnixTimeSeconds(security.RegularMarketTime).UtcDateTime;
            var estNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, _estZone);
            var estMarketTime = TimeZoneInfo.ConvertTimeFromUtc(marketTime, _estZone);

            if (estMarketTime.Day != estNow.Day)
            {
                _logger.LogWarning("Invalid Day");
                return;
            }

            var imageByte = await CreateImage(security);
            var content = BuildContent(symbol, security, estMarketTime);

            try
            {
                _logger.LogInformation("post tweet using X API");
                await tweetService.PostTweet(content, imageByte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while posting tweet");
            }

            try
            {
                _logger.LogInformation("send slack message using Slack API");
                await slackService.SendMessage(stock.SlackChannel, symbol, content, imageByte);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while sending slack message");
            }
        }
    }

    private async Task<IReadOnlyDictionary<string, Security>> GetStockInfo(List<Stock> stocks)
    {
        try
        {
            // JJ: 원하는 시각에 종가 검색 기능 확인
            var stockInfo = await YahooFinanceService.GetPriceInfo([.. stocks.Select(s => s.Symbol)]);
            if (stockInfo is null || stockInfo.Count == 0)
            {
                throw new InvalidOperationException("No stock information found");
            }

            _logger.LogInformation("Stock information retrieved successfully: {StockInfo}", string.Join(", ", stockInfo.Keys));

            return stockInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching stock prices");
            throw;
        }
    }

    private async Task<byte[]?> CreateImage(Security security)
    {
        _logger.LogInformation("create image using OpenAI API");

        var sentiment = security.RegularMarketChange switch
        {
            > 0 => MarketSentiment.Positive,
            < 0 => MarketSentiment.Negative,
            _ => MarketSentiment.Neutral,
        };

        var prompt = openAIService.GetPrompt(
            sentiment,
            security.ShortName,
            $"{security.RegularMarketPrice:N2}",
            $"{security.RegularMarketChange:N2}"
        );

        try
        {
            return await openAIService.CreateImage(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating image");
            return null;
        }
    }

    private static string BuildContent(string symbol, Security security, DateTime estMarketTime)
    {
        var sign = security.RegularMarketChange > 0 ? "+" : "";
        var marketCap = security.MarketCap >= 1_000_000_000_000
            ? $"{security.MarketCap / 1_000_000_000_000:N2}T"
            : $"{security.MarketCap / 1_000_000_000:N2}B";

        return $"""
            [{estMarketTime:yyyy-MM-dd}]
            ${symbol}
            ${security.RegularMarketPrice:N2}
            {sign}${security.RegularMarketChange:N2} ({sign}{security.RegularMarketChangePercent:N2}%)
            ${marketCap}
            """;
    }
}
