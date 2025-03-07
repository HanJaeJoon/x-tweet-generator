using TweetGenerator.Services;

namespace TweetGenerator.Tests.Tests;

public class YahooFinanceTests
{
    private static readonly string[] _symbols = ["TSLA"];

    [Fact]
    public async Task Get_StockInfo_SuccessfullyAsync()
    {
        var result = await YahooFinanceService.GetPriceInfo(_symbols);

        Assert.NotNull(result);
    }
}