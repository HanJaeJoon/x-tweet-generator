using Microsoft.Extensions.Logging;
using YahooFinanceApi;

namespace TweetGenerator.Services;

public class YahooFinanceService(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<YahooFinanceService>();
    private static readonly Field[] _fields = [
        Field.MarketState,
        Field.Symbol, Field.ShortName,
        Field.RegularMarketTime, Field.RegularMarketPrice, Field.RegularMarketChange, Field.RegularMarketChangePercent
    ];

    public async Task<(string, string, DateTime, double, double, double)> GetPriceInfo(string symbol)
    {
        var security = await Yahoo.Symbols(symbol).Fields(_fields).QueryAsync();

        var stockInfo = security[symbol];
        var marketState = stockInfo.MarketState;
        var stockName = stockInfo.ShortName;
        var currentPrice = stockInfo.RegularMarketPrice;
        var change = stockInfo.RegularMarketChange;
        var changePercent = stockInfo.RegularMarketChangePercent;
        var regularMarketTime = stockInfo.RegularMarketTime;

        return (marketState, stockName, DateTimeOffset.FromUnixTimeSeconds(regularMarketTime).UtcDateTime, currentPrice, change, changePercent);
    }

    public async Task GetStockHistory(string symbol)
    {
        var history = await Yahoo.GetHistoricalAsync(symbol, DateTime.Today.AddDays(-1), DateTime.Today, Period.Daily);

        foreach (var candle in history)
        {
            _logger.LogInformation($"DateTime: {candle.DateTime}, Open: {candle.Open}, High: {candle.High}, Low: {candle.Low}, Close: {candle.Close}, Volume: {candle.Volume}, AdjustedClose: {candle.AdjustedClose}");
        }
    }
}
