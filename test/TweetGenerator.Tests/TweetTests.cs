using TweetGenerator.Services;
using TweetGenerator.Tests.Fixtures;

namespace TweetGenerator.Tests;

public class TweetTests(ConfigurationFixture fixture) : IClassFixture<ConfigurationFixture>
{
    private readonly TweetService _tweetSerivce = new(fixture.Configuration);

    [Fact]
    public async Task PostTweet_WithImage_Successfully()
    {
        var content = "API 테스트 중입니다.";
        using var httpClient = new HttpClient();
        var imageByte = await httpClient.GetByteArrayAsync("https://www.kasandbox.org/programming-images/avatars/purple-pi.png");

        var tweetId = await _tweetSerivce.PostTweet(content, imageByte);

        Assert.True(long.TryParse(tweetId, out _));
    }

    [Fact]
    public async Task GetTweetInfo_ByTweetId_Successfully()
    {
        var tweetId = 1747990594163810427;
        var tweet = await _tweetSerivce.GetTweetInfo(tweetId);
    }
}