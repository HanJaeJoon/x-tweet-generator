using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SlackNet;
using SlackNet.WebApi;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TweetGenerator.Services;

public class SlackService(IConfiguration configuration, ILoggerFactory loggerFactory)
{
    private readonly string _slackToken = configuration["SlackToken"] ?? throw new InvalidOperationException();
    private readonly string _openAIImageModel = configuration["OpenAIImageModel"] ?? throw new InvalidOperationException();

    private readonly ILogger _logger = loggerFactory.CreateLogger<Function>();

    public async Task SendMessage(string channel, string symbol, string content, byte[]? imageByte = null)
    {
        var slackClient = new SlackServiceBuilder()
            .UseApiToken(_slackToken)
            .GetApiClient();

        var channels = await slackClient.Conversations.List(true);
        var channelId = channels?.Channels?.FirstOrDefault(x => x.Name == channel.Replace("#", ""))?.Id;

        if (channelId is null)
        {
            _logger.LogError("Channel not found: {channel}", channel);
            return;
        }

        bool imageUploaded = false;

        if (imageByte?.Length > 0)
        {
            try
            {
                var urlResponse = await slackClient.Files.GetUploadUrlExternal(fileName: $"{symbol}{DateTime.UtcNow:yyyyMMdd}", length: imageByte.Length)
                    ?? throw new InvalidOperationException("Failed to get upload URL from Slack");

                using var httpClient = new HttpClient();
                using var fileContent = new ByteArrayContent(imageByte);

                fileContent.Headers.Add("Content-Type", "application/octet-stream");

                var uploadResponse = await httpClient.PostAsync(urlResponse.UploadUrl, fileContent);

                uploadResponse.EnsureSuccessStatusCode();

                var completeResponse = await slackClient.Files.CompleteUploadExternal(
                    files: [new()
                    {
                        Id = urlResponse.FileId,
                        Title = $"Image of {symbol} stock generated with {_openAIImageModel} in Open AI",
                    }],
                    channelId: channelId,
                    initialComment: content
                );

                if (!completeResponse.Any())
                {
                    throw new InvalidOperationException("Failed to complete file upload to Slack");
                }

                imageUploaded = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during image upload");
            }
        }

        if (!imageUploaded)
        {
            await slackClient.Chat.PostMessage(new Message { Channel = channelId, Text = content });
        }
    }
}
