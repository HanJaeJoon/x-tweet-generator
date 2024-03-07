using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Services;
using TweetGenerator.Tests.Fixtures;

namespace TweetGenerator.Tests.Tests;

public class YahooFinanceTests(YahooFinanceServiceFixture fixture) : IClassFixture<YahooFinanceServiceFixture>
{
    private readonly YahooFinanceService _yahooFinanceService = fixture.ServiceProvider.GetRequiredService<YahooFinanceService>();

    private static readonly string[] _symbols = ["TSLA"];

    [Fact]
    public async Task Get_StockInfo_SuccessfullyAsync()
    {
        var result = await _yahooFinanceService.GetPriceInfo(_symbols);

        Assert.NotNull(result);
    }
}