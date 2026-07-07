using Microsoft.Extensions.Configuration;
using OpenAI.Images;

namespace TweetGenerator.Services;

public enum MarketSentiment
{
    Positive,
    Neutral,
    Negative,
}

public class OpenAIService(IConfiguration configuration)
{
    private readonly ImageClient _imageClient = new(
        configuration["OpenAIImageModel"] ?? throw new InvalidOperationException("OpenAIImageModel is not configured"),
        configuration["OpenAIApiKey"] ?? throw new InvalidOperationException("OpenAIApiKey is not configured")
    );

    private const string PositivePrompt = """
        Generate an image of a [character] in a modern office, reacting with excitement to the rise of [stockName]'s stock price.
        The digital display on the wall shows [stockName]'s stock price at $[currentPrice], with a vibrant candlestick chart showing a clear upward trend.
        For example, the price of the stock increased by $[priceChange] to $[currentPrice]. If the company name [stockName] cannot be rendered accurately, omit it entirely from the display.
        The [character]'s joy—expressed through smiling, cheering, or jumping—should scale with the stock price increase from a baseline, with subtle excitement for small gains and wild enthusiasm for large ones.
        For larger increases, add dynamic background elements like confetti falling, colleagues cheering, or a festive atmosphere, while keeping the office setting professional with desks, computers, and financial charts.
    """;
    private const string NeutralPrompt = """
        Generate an image of a [character] in a modern office, reacting with calm indifference to [stockName]'s stock price staying flat.
        The digital display on the wall shows [stockName]'s stock price at $[currentPrice], with a candlestick chart showing a sideways, flat trend.
        For example, the price of the stock stayed unchanged at $[currentPrice]. If the company name [stockName] cannot be rendered accurately, omit it entirely from the display.
        The [character]'s reaction should be relaxed and neutral, such as shrugging, sipping coffee, or glancing at the screen without much concern.
        Keep the office setting professional and quiet, with desks, computers, and financial charts, and a calm everyday atmosphere.
    """;
    private const string NegativePrompt = """
        Generate an image of a [character] in a modern office, reacting with despair to a decline in [stockName]'s stock price.
        The computer screen prominently displays [stockName]'s stock price at $[currentPrice], with a detailed candlestick chart showing a sharp downward trend.
        For example, the price of the stock dropped by $[priceChange] to $[currentPrice]. If the company name [stockName] cannot be rendered accurately, omit it entirely from the screen.
        The [character]'s sadness—expressed through actions like head in hands, slumped posture, or a distraught expression—should scale with the stock price drop from a baseline, with mild disappointment for small declines and deep despair for large ones.
        For larger decreases, intensify the background with elements like scattered papers across the desk, a dimly lit room, or a chaotic office atmosphere, while maintaining a professional setting with computers, files, and financial charts.
    """;
    private static readonly string[] _characters = [
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

    public string GetPrompt(MarketSentiment sentiment, string stockName, string currentPrice, string priceChange)
    {
        var prompt = sentiment switch
        {
            MarketSentiment.Positive => PositivePrompt,
            MarketSentiment.Neutral => NeutralPrompt,
            _ => NegativePrompt,
        };
        var character = _characters[Random.Shared.Next(_characters.Length)];

        return prompt
            .Replace("[character]", character)
            .Replace("[stockName]", stockName)
            .Replace("[currentPrice]", currentPrice)
            .Replace("[priceChange]", priceChange);
    }

    public async Task<byte[]> CreateImage(string prompt)
    {
        var options = new ImageGenerationOptions
        {
            Size = GeneratedImageSize.W1024xH1024,
        };

        GeneratedImage image = await _imageClient.GenerateImageAsync(prompt, options);

        return image.ImageBytes.ToArray();
    }
}
