using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using TweetGenerator.Models;
using TweetGenerator.Services;
using TweetGenerator.Tests.Fixtures;

namespace TweetGenerator.Tests.Tests;

public class SlackTests : IClassFixture<SlackServiceFixture>
{
    private readonly SlackService _slackService;
    private readonly IConfiguration _configuration;
    private readonly List<Stock> _stocks;

    public SlackTests(SlackServiceFixture fixture)
    {
        _slackService = fixture.ServiceProvider.GetRequiredService<SlackService>();
        _configuration = fixture.ServiceProvider.GetRequiredService<IConfiguration>();
        _stocks = JsonSerializer.Deserialize<List<Stock>>(_configuration["Stocks"] ?? "[]") ?? []; ;
    }

    [Fact]
    public async Task SendMessage_Successfully()
    {
        using var httpClient = new HttpClient();
        var byteArray = await httpClient.GetByteArrayAsync("https://placehold.co/600x400.jpg");

        foreach (var stock in _stocks)
        {
            await _slackService.SendMessage(stock.SlackChannel, stock.Symbol, "일희일비봇 테스트 중입니다.", byteArray);
        }
    }
}