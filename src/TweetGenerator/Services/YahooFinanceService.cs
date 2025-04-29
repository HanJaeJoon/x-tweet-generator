using YahooFinanceApi;

namespace TweetGenerator.Services;

public class YahooFinanceService
{
    private static readonly Field[] _fields = [
        Field.MarketState,
        Field.Symbol, Field.ShortName,
        Field.RegularMarketTime, Field.RegularMarketPrice, Field.RegularMarketChange, Field.RegularMarketChangePercent,
        Field.MarketCap,
    ];

    public static async Task<IReadOnlyDictionary<string, Security>?> GetPriceInfo(string[] symbols)
    {
        var securities = await Yahoo
            .Symbols(symbols)
            .Fields(_fields)
            .QueryAsync();

        return securities;
    }

    public static async Task<decimal?> GetClosingPrice(string symbol, DateTime date)
    {
        var startDate = date.Date;
        var endDate = date.Date.AddDays(1).AddTicks(-1);

        var history = await Yahoo.GetHistoricalAsync(symbol, startDate, endDate);

        if (history != null && history.Any())
        {
            return history[0].Close;
        }

        return null;
    }
}
