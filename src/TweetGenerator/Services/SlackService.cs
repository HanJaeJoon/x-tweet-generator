using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SlackAPI;
using TweetGenerator.utils;

namespace TweetGenerator.Services;

public class SlackService(IConfiguration configuration, ILoggerFactory loggerFactory)
{
    private readonly string _slackToken = configuration["SlackToken"] ?? throw new InvalidOperationException();
    private readonly string _channel = configuration["SlackChannel"] ?? throw new InvalidOperationException();

    private readonly ILogger _logger = loggerFactory.CreateLogger<Function>();

    public async Task SendMessage(string content, byte[]? imageByte = null)
    {
        var slackClient = new SlackTaskClient(_slackToken);

        if (imageByte is not null)
        {
            var fileUploadResponse = await slackClient.UploadFileAsync(
                fileData: imageByte,
                fileName: $"{DateTime.UtcNow:yyyyMMdd}",
                channelIds: [_channel],
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

        var messageResponse = await slackClient.PostMessageAsync(_channel, content);

        if (!messageResponse.ok)
        {
            _logger.LogError("Message sending failed: {error}", messageResponse.error);
        }
    }

    public async Task UploadFile(byte[] image)
    {
        var client = new SlackTaskClient(_slackToken);
        var compressedImage = ImageHelper.CompressImage(image);
        var result = await client.UploadFileAsync(compressedImage, "test.jpg", [_channel], title: "테스트", initialComment: "테스트 이미지");
    }
}
