using Microsoft.Extensions.Configuration;
using OpenAI_API;

namespace TweetGenerator.Services;

public class OpenAiService(IConfiguration configuration)
{
    private const string _positivePrompt = """
        Generate an image of a [character] inin an office, reacting to rise of [stockName]'s stock price.
        The digital display shows [stockName]'s stock price at $[currentPrice], with a green arrow indicating an upward trend.
        The intensity of the person's joy (smiling, cheering, jumping) should correlate with the stock price increase from a baseline
        (e.g., The price of the stock increased by $[priceChange] to $[currentPrice])
        Background elements like confetti or colleagues celebrating can be added for larger increases.
    """;
    private const string _negativePrompt = """
        Generate an image of a [character] in an office, reacting to a decline in [stockName]'s stock price.
        The computer screen displays [stockName]'s stock at $[currentPrice], with a red arrow showing a downward trend.
        The degree of the person's sadness (head in hands, slumped posture, distraught expression) should reflect the stock price drop from a baseline
        (e.g., The price of the stock dropped by $[priceChange] to $[currentPrice])
        Background elements like scattered papers or a dimly lit room can intensify for larger decreases.
    """;

    private readonly string _apiKey = configuration["OpenAiApiKey"] ?? throw new InvalidOperationException();
    private readonly string[] _characters = [
        "person (female, Asian)", "person (male, Asian)",
        "person (female, African)", "person (male, African)",
        "person (female, Caucasian)", "person (male, Caucasian)",
        "person (female, Hispanic)", "person (male, Hispanic)",
        "person (female, Middle-Eastern)", "person (male, Middle-Eastern)",
        "person (female, South Asian)", "person (male, South Asian)",
        "old man", "old woman",
        "cat", "dog", "pig"
    ];

    public string GetPrompt(bool isPositive)
    {
        var prompt = isPositive ? _positivePrompt : _negativePrompt;

        var random = new Random();
        var index = random.Next(_characters.Length);

        prompt = prompt
            .Replace("[character]", _characters[index])
            ;

        return prompt;
    }

    public async Task<byte[]> CreateImage(string prompt)
    {
        var api = new OpenAIAPI(_apiKey);
        var result = await api.ImageGenerations.CreateImageAsync(prompt, OpenAI_API.Models.Model.DALLE3);
        var url = result.Data[0].Url;

        using var httpClient = new HttpClient();
        var byteArray = await httpClient.GetByteArrayAsync(url);

        return byteArray;
    }
}
