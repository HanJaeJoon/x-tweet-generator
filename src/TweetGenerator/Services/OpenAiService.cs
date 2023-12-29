using Microsoft.Extensions.Configuration;
using OpenAI_API;

namespace TweetGenerator.Services;

public class OpenAiService(IConfiguration configuration)
{
    private readonly string _apiKey = configuration["OpenAiApiKey"] ?? throw new InvalidOperationException();
    private readonly string _prompt = """
        Flat simple vector illustration style, vibrant colors, white background,
        Male programmer sitting in front of laptop, rejoicing in the rise of stock
    """;

    public async Task<byte[]> CreateImage()
    {
        var api = new OpenAIAPI(_apiKey);
        var result = await api.ImageGenerations.CreateImageAsync(_prompt, OpenAI_API.Models.Model.DALLE3);
        var url = result.Data[0].Url;

        using var httpClient = new HttpClient();
        var byteArray = await httpClient.GetByteArrayAsync(url);

        return byteArray;
    }
}
