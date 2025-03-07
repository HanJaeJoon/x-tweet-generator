using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Services;
using TweetGenerator.Tests.Fixtures;

namespace TweetGenerator.Tests.Tests;

public class TweetTests(TweetServiceFixture fixture) : IClassFixture<TweetServiceFixture>
{
    private readonly TweetService _tweetService = fixture.ServiceProvider.GetRequiredService<TweetService>();

    [Fact]
    public async Task PostTweet_WithImage_Successfully()
    {
        var content = "API 테스트 중입니다.";
        using var httpClient = new HttpClient();
        var imageByte = await httpClient.GetByteArrayAsync("https://placehold.co/600x400.jpg");

        var tweetId = await _tweetService.PostTweet(content, imageByte);

        Assert.True(long.TryParse(tweetId, out _));
    }

    [Fact]
    public async Task GetTweetInfo_ByTweetId_Successfully()
    {
        var tweetId = 1747990594163810427;
        var tweet = await _tweetService.GetTweetInfo(tweetId);

        Assert.NotNull(tweet);
    }
}