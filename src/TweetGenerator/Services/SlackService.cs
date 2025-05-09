﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SlackAPI;

namespace TweetGenerator.Services;

public class SlackService(IConfiguration configuration, ILoggerFactory loggerFactory)
{
    private readonly string _slackToken = configuration["SlackToken"] ?? throw new InvalidOperationException();
    private readonly string _openAIImageModel = configuration["OpenAIImageModel"] ?? throw new InvalidOperationException();

    private readonly ILogger _logger = loggerFactory.CreateLogger<Function>();

    public async Task SendMessage(string channel, string symbol, string content, byte[]? imageByte = null)
    {
        var slackClient = new SlackTaskClient(_slackToken);

        if (imageByte is not null)
        {
            var fileUploadResponse = await slackClient.UploadFileAsync(
                fileData: imageByte,
                fileName: $"{symbol}{DateTime.UtcNow:yyyyMMdd}",
                channelIds: [channel],
                title: $"Image of {symbol} stock generated with {_openAIImageModel} in Open AI"
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
