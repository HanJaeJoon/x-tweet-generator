using OpenAI_API;

namespace XGenerator.Services;

public class OpenAiService(OpenAiConfiguration configuration)
{
    public async Task<byte[]> GetImage(string prompt)
    {
        var api = new OpenAIAPI(configuration.ApiKey);
        var result = await api.ImageGenerations.CreateImageAsync(prompt, OpenAI_API.Models.Model.DALLE3);
        var url = result.Data[0].Url;

        using var httpClient = new HttpClient();
        var byteArray = await httpClient.GetByteArrayAsync(url);

        return byteArray;
    }
}
