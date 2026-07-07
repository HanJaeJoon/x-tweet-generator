using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.WebApi;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TweetGenerator.Services;

public class SlackService(IConfiguration configuration, ILogger<SlackService> logger)
{
    private static readonly HttpClient _httpClient = new();

    private readonly string _imageModel = configuration["OpenAIImageModel"] ?? throw new InvalidOperationException();
    private readonly ISlackApiClient _slackClient = new SlackServiceBuilder()
        .UseApiToken(configuration["SlackToken"] ?? throw new InvalidOperationException())
        .GetApiClient();

    private readonly ILogger _logger = logger;

    public async Task SendMessage(string channel, string symbol, string content, byte[]? imageByte = null)
    {
        var channelName = channel.Replace("#", "");
        var channels = await _slackClient.Conversations.List(true);
        var channelId = channels?.Channels?.FirstOrDefault(x => string.Equals(x.Name, channelName, StringComparison.OrdinalIgnoreCase))?.Id;

        if (channelId is null)
        {
            _logger.LogError("Channel not found: {Channel}", channel);
            return;
        }

        var imageUploaded = false;

        if (imageByte?.Length > 0)
        {
            try
            {
                await UploadImage(channelId, symbol, content, imageByte);
                imageUploaded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during image upload");
            }
        }

        if (!imageUploaded)
        {
            await _slackClient.Chat.PostMessage(new Message { Channel = channelId, Text = content });
        }
    }

    private async Task UploadImage(string channelId, string symbol, string content, byte[] imageByte)
    {
        var urlResponse = await _slackClient.Files.GetUploadUrlExternal(fileName: $"{symbol}{DateTime.UtcNow:yyyyMMdd}", length: imageByte.Length)
            ?? throw new InvalidOperationException("Failed to get upload URL from Slack");

        using var fileContent = new ByteArrayContent(imageByte);

        fileContent.Headers.Add("Content-Type", "application/octet-stream");

        var uploadResponse = await _httpClient.PostAsync(urlResponse.UploadUrl, fileContent);

        uploadResponse.EnsureSuccessStatusCode();

        var completeResponse = await _slackClient.Files.CompleteUploadExternal(
            files: [new()
            {
                Id = urlResponse.FileId,
                Title = $"Image of {symbol} stock generated with {_imageModel} in Open AI",
            }],
            channelId: channelId,
            initialComment: content
        );

        if (!completeResponse.Any())
        {
            throw new InvalidOperationException("Failed to complete file upload to Slack");
        }
    }
}
