using Microsoft.Extensions.Logging;
using YahooFinanceApi;

namespace TweetGenerator.Services;

public class YahooFinanceService(ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<YahooFinanceService>();
    private const string _symbol = "TSLA";

    public async Task<(double, double, double)> GetPriceInfo()
    {
        var security = await Yahoo.Symbols(_symbol)
            .Fields(Field.Symbol, Field.RegularMarketPrice, Field.RegularMarketChange, Field.RegularMarketChangePercent)
            .QueryAsync();
        var tsla = security[_symbol];
        var currentPrice = tsla.RegularMarketPrice;
        var change = tsla.RegularMarketChange;
        var changePercent = tsla.RegularMarketChangePercent;

        //var history = await Yahoo.GetHistoricalAsync(_symbol, DateTime.Today.AddDays(-1), DateTime.Today, Period.Daily);

        //foreach (var candle in history)
        //{
        //    _logger.LogInformation($"DateTime: {candle.DateTime}, Open: {candle.Open}, High: {candle.High}, Low: {candle.Low}, Close: {candle.Close}, Volume: {candle.Volume}, AdjustedClose: {candle.AdjustedClose}");
        //}

        return (currentPrice, change, changePercent);
    }
}
