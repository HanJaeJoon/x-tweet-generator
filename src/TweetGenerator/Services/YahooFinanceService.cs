using YahooFinanceApi;

namespace TweetGenerator.Services;

public class YahooFinanceService
{
    private static readonly Field[] _fields = [
        Field.MarketState,
        Field.Symbol, Field.ShortName,
        Field.RegularMarketTime, Field.RegularMarketPrice, Field.RegularMarketChange, Field.RegularMarketChangePercent
    ];

    public static async Task<IReadOnlyDictionary<string, Security>> GetPriceInfo(string[] symbols)
    {
        var securities = await Yahoo.Symbols(symbols).Fields(_fields).QueryAsync();

        return securities;
    }
}
