using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Services;
using TweetGenerator.Tests.Fixtures;

namespace TweetGenerator.Tests.Tests;

public class OpenAITests(OpenAIServiceFixture fixture) : IClassFixture<OpenAIServiceFixture>
{
    private readonly OpenAIService _openAIService = fixture.ServiceProvider.GetRequiredService<OpenAIService>();

    [Fact]
    public async Task CreateImage_Successfully()
    {
        var prompt = """
        Generate an image of a CEO (female) in a modern office, reacting with despair to a decline in Tesla, Inc.'s stock price.
        The computer screen prominently displays Tesla, Inc.'s stock price at $283.55, with a detailed candlestick chart showing a sharp downward trend.
        For example, the price of the stock dropped by $-2.33 to $283.55. If the company name Tesla, Inc. cannot be rendered accurately, omit it entirely from the screen.
        The CEO (female)'s sadness—expressed through actions like head in hands, slumped posture, or a distraught expression—should scale with the stock price drop from a baseline, with mild disappointment for small declines and deep despair for large ones.
        For larger decreases, intensify the background with elements like scattered papers across the desk, a dimly lit room, or a chaotic office atmosphere, while maintaining a professional setting with computers, files, and financial charts.
        """;

        var data = await _openAIService.CreateImage(prompt);

        Assert.NotEmpty(data);
        Assert.True(data.Length > 0);
    }
}
