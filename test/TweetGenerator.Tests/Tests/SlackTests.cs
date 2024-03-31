using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Services;
using TweetGenerator.Tests.Fixtures;

namespace TweetGenerator.Tests.Tests;

public class SlackTests(SlackServiceFixture fixture) : IClassFixture<SlackServiceFixture>
{
    private readonly SlackService _slackService = fixture.ServiceProvider.GetRequiredService<SlackService>();

    [Fact]
    public async Task UploadFile_Successfully()
    {
        var testImagePath = "Assets/sample.png";
        var fileContent = File.ReadAllBytes(testImagePath);

        await _slackService.UploadFile(fileContent);
    }
}