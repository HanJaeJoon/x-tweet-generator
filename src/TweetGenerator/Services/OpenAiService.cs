using Microsoft.Extensions.Configuration;
using OpenAI.Images;

namespace TweetGenerator.Services;

public class OpenAIService(IConfiguration configuration)
{
    private const string _positivePrompt = """
        Generate an image of a [character] in a modern office, reacting with excitement to the rise of [stockName]'s stock price.
        The digital display on the wall shows [stockName]'s stock price at $[currentPrice], with a vibrant candlestick chart showing a clear upward trend.
        For example, the price of the stock increased by $[priceChange] to $[currentPrice]. If the company name [stockName] cannot be rendered accurately, omit it entirely from the display.
        The [character]'s joy—expressed through smiling, cheering, or jumping—should scale with the stock price increase from a baseline, with subtle excitement for small gains and wild enthusiasm for large ones.
        For larger increases, add dynamic background elements like confetti falling, colleagues cheering, or a festive atmosphere, while keeping the office setting professional with desks, computers, and financial charts.
    """;
    private const string _negativePrompt = """
        Generate an image of a [character] in a modern office, reacting with despair to a decline in [stockName]'s stock price.
        The computer screen prominently displays [stockName]'s stock price at $[currentPrice], with a detailed candlestick chart showing a sharp downward trend.
        For example, the price of the stock dropped by $[priceChange] to $[currentPrice]. If the company name [stockName] cannot be rendered accurately, omit it entirely from the screen.
        The [character]'s sadness—expressed through actions like head in hands, slumped posture, or a distraught expression—should scale with the stock price drop from a baseline, with mild disappointment for small declines and deep despair for large ones.
        For larger decreases, intensify the background with elements like scattered papers across the desk, a dimly lit room, or a chaotic office atmosphere, while maintaining a professional setting with computers, files, and financial charts.
    """;

    private readonly string _apiKey = configuration["OpenAiApiKey"] ?? throw new InvalidOperationException();
    private readonly string[] _characters = [
        "stock trader (male)", "stock trader (female)",
        "CEO (male)", "CEO (female)",
        "intern (male)", "intern (female)",
        "optimist (male)", "optimist (female)",
        "pessimist (male)", "pessimist (female)",
        "drama queen", "drama king",
        "wizard (male)", "wizard (female)",
        "alien (male)", "alien (female)",
        "cat in a suit",
        "dog with glasses",
        "pig with a tie",
        "bear in a vest",
        "bull with a briefcase",
        "owl with a calculator",
        "fox in a trench coat",
        "rabbit with a stopwatch",
        "penguin in a bowtie"
    ];

    public string GetPrompt(bool isPositive)
    {
        var prompt = isPositive ? _positivePrompt : _negativePrompt;

        var index = Random.Shared.Next(_characters.Length);

        prompt = prompt
            .Replace("[character]", _characters[index])
        ;

        return prompt;
    }

    public async Task<byte[]> CreateImage(string prompt)
    {
        var imageClient = new ImageClient("dall-e-3", _apiKey);
        var options = new ImageGenerationOptions()
        {
            Quality = GeneratedImageQuality.High,
            Size = GeneratedImageSize.W1024xH1024,
            Style = GeneratedImageStyle.Vivid,
            ResponseFormat = GeneratedImageFormat.Bytes,
        };

        GeneratedImage image = await imageClient.GenerateImageAsync(prompt, options);

        return image.ImageBytes.ToArray();
    }
}
