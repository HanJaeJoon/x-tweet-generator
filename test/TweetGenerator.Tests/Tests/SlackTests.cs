using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Services;
using TweetGenerator.Tests.Fixtures;

namespace TweetGenerator.Tests.Tests;

public class SlackTests(SlackServiceFixture fixture) : IClassFixture<SlackServiceFixture>
{
    private readonly SlackService _slackService = fixture.ServiceProvider.GetRequiredService<SlackService>();

    [Fact]
    public async Task SendMessage_Successfully()
    {
        using var httpClient = new HttpClient();
        var byteArray = await httpClient.GetByteArrayAsync("https://placehold.co/600x400.jpg");
        await _slackService.SendMessage("일희일비봇 테스트 중입니다.", byteArray);
    }
}