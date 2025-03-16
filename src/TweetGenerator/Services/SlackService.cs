using Microsoft.Extensions.Logging;
using SlackAPI;
using TweetGenerator.Models;

namespace TweetGenerator.Services;

public class SlackService(Config config, ILoggerFactory loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Function>();

    public async Task SendMessage(string channel, string symbol, string content, byte[]? imageByte = null)
    {
        var slackClient = new SlackTaskClient(config.SlackToken);

        if (imageByte is not null)
        {
            var fileUploadResponse = await slackClient.UploadFileAsync(
                fileData: imageByte,
                fileName: $"{symbol}{DateTime.UtcNow:yyyyMMdd}",
                channelIds: [channel],
                title: "image created with DALL-E 3 in Open AI"
            );

            if (!fileUploadResponse.ok)
            {
                _logger.LogError("File upload failed: {error}", fileUploadResponse.error);
                return;
            }

            var fileUrl = fileUploadResponse.file.permalink;

            content += $"\n{fileUrl}";
        }

        var messageResponse = await slackClient.PostMessageAsync(channel, content);

        if (!messageResponse.ok)
        {
            _logger.LogError("Message sending failed: {error}", messageResponse.error);
        }
    }
}
