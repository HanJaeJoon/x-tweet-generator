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

public class Function(IConfiguration configuration, ILoggerFactory loggerFactory, OpenAIService openAiService, TweetService tweetService, SlackService slackService)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Function>();

    [Function("GenerateTweet")]
    public async Task RunScheduled([TimerTrigger("30 0 16 * * Mon-Fri")] TimerInfo timerInfo)
    {
        var utcNow = timerInfo.ScheduleStatus?.Last ?? DateTime.UtcNow;
        _logger.LogInformation("Timer triggered at: {now}", utcNow);

        await RunImpl(utcNow);
    }

    [Function("ManualTrigger")]
    public async Task<HttpResponseData> RunManual([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequestData req, FunctionContext _)
    {
        if (!DateTimeOffset.TryParse(req.Query["utcTime"], out var utcTime))
        {
            _logger.LogError("Invalid dateTime format");
            var invalidDateResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            invalidDateResponse.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            invalidDateResponse.WriteString("Invalid dateTime format");
            return invalidDateResponse;
        }

        _logger.LogInformation("Manually triggered at: {now}", utcTime);

        await RunImpl(utcTime.UtcDateTime);

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        response.WriteString("Success !!!");

        return response;
    }

    private async Task RunImpl(DateTime utcNow)
    {
        var stocks = JsonSerializer.Deserialize<List<Stock>>(configuration["Stocks"] ?? "[]") ?? [];
        _logger.LogInformation("Starting tweet generation for {count} stocks at {now}", stocks.Count, utcNow);

        _logger.LogDebug("get stock price using Yahoo Finance API at {time}", utcNow);

        IReadOnlyDictionary<string, Security>? stockInfo = null;

        try
        {
            // JJ: 원하는 시각에 종가 검색 기능 확인
            stockInfo = await YahooFinanceService.GetPriceInfo([.. stocks.Select(s => s.Symbol)]);
            if (stockInfo is null || stockInfo.Count == 0)
            {
                throw new Exception("No stock information found");
            }
            _logger.LogInformation("Stock information retrieved successfully: {stockInfo}", string.Join(", ", stockInfo.Keys));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error occurred while fetching stock prices: {exception}", ex.Message);
            throw;
        }

        foreach (var stock in stocks)
        {
            var symbol = stock.Symbol;
            _logger.LogInformation("Processing stock: {symbol}", stock.Symbol);

            if (!stockInfo.TryGetValue(symbol, out var security))
            {
                _logger.LogWarning("Stock information not found for symbol: {symbol}", symbol);
                _logger.LogInformation("stockInfo: {stockInfo}", JsonSerializer.Serialize(stockInfo));
                continue;
            }

            if (security.MarketState is "REGULAR")
            {
                _logger.LogWarning("Market is open (MarketState: {marketState})", security.MarketState);
                return;
            }

            var marketTime = DateTimeOffset.FromUnixTimeSeconds(security.RegularMarketTime).UtcDateTime;
            var estZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            var estNow = TimeZoneInfo.ConvertTimeFromUtc(utcNow, estZone);
            var estMarketTime = TimeZoneInfo.ConvertTimeFromUtc(marketTime, estZone);

            if (estMarketTime.Day != estNow.Day)
            {
                _logger.LogWarning("Invalid Day");
                return;
            }

            _logger.LogInformation($"create image using OpenAI API");

            var prompt = openAiService.GetPrompt(security.RegularMarketChange > 0)
                .Replace("[stockName]", security.ShortName)
                .Replace("[currentPrice]", string.Format("{0:N2}", security.RegularMarketPrice))
                .Replace("[priceChange]", string.Format("{0:N2}", security.RegularMarketChange))
            ;

            byte[]? imageByte = null;

            try
            {
                imageByte = await openAiService.CreateImage(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while creating image: {exception}", ex.Message);
            }

            var content = $"""
                [{estMarketTime:yyyy-MM-dd}]
                the stock price of ${symbol}
                ${string.Format("{0:N2}", security.RegularMarketPrice)}
                {(security.RegularMarketChange > 0 ? "+" : "")}{string.Format("{0:N2}", security.RegularMarketChange)} ({string.Format("{0:N2}", security.RegularMarketChangePercent)}%)
            """;

            try
            {
                _logger.LogInformation("post tweet using X API");
                await tweetService.PostTweet(content, imageByte);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while posting tweet: {exception}", ex.Message);
            }

            try
            {
                _logger.LogInformation("send slack message using Slack API");
                await slackService.SendMessage(stock.SlackChannel, symbol, content, imageByte);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error occurred while sending slack message: {exception}", ex.Message);
            }
        }
    }
}
