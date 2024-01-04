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

    public async Task<IList<(string, string, DateTime, double, double, double)>> GetPriceInfo(string[] symbols)
    {
        var security = await Yahoo.Symbols(symbols).Fields(_fields).QueryAsync();

        List<(string, string, DateTime, double, double, double)> results = [];

        foreach (var symbol in symbols)
        {
            var stockInfo = security[symbol];
            var marketState = stockInfo.MarketState;
            var stockName = stockInfo.ShortName;
            var currentPrice = stockInfo.RegularMarketPrice;
            var change = stockInfo.RegularMarketChange;
            var changePercent = stockInfo.RegularMarketChangePercent;
            var regularMarketTime = stockInfo.RegularMarketTime;

            results.Add((marketState, stockName, DateTimeOffset.FromUnixTimeSeconds(regularMarketTime).UtcDateTime, currentPrice, change, changePercent));
        }

        return results;
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
