using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Models;
using TweetGenerator.Services;
using TweetGenerator.Tests.Fixtures;

namespace TweetGenerator.Tests.Tests;

public class SlackTests(SlackServiceFixture fixture) : IClassFixture<SlackServiceFixture>
{
    private readonly SlackService _slackService = fixture.ServiceProvider.GetRequiredService<SlackService>();
    private readonly Config _config = fixture.ServiceProvider.GetRequiredService<Config>();

    [Fact]
    public async Task SendMessage_Successfully()
    {
        using var httpClient = new HttpClient();
        var byteArray = await httpClient.GetByteArrayAsync("https://placehold.co/600x400.jpg");

        foreach (var stock in _config.Stocks)
        {
            await _slackService.SendMessage(stock.SlackChannel, stock.Symbol, "일희일비봇 테스트 중입니다.", byteArray);
        }
    }
}