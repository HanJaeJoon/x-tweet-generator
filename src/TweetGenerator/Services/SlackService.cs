using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SlackAPI;

namespace TweetGenerator.Services;

public class SlackService(IConfiguration configuration, ILoggerFactory loggerFactory)
{
    private readonly string _slackToken = configuration["SlackToken"] ?? throw new InvalidOperationException();
    private readonly string _channel = configuration["SlackChannel"] ?? throw new InvalidOperationException();

    private readonly ILogger _logger = loggerFactory.CreateLogger<Function>();

    public async Task SendMessage(string content, string imageUrl)
    {
        var client = new SlackTaskClient(_slackToken);
        var attachment = new Attachment()
        {
            image_url = imageUrl,
            fallback = "(image created with DALL-E 3 in Open AI)",
        };

        // JJ: DALL-E 3 최소 용량으로 이미지 생성해도 3MB가 넘어서 슬랙에서 안 보이는 문제
        // html trigger 사용 하는 방법?
        var response = await client.PostMessageAsync(_channel, content, attachments: [attachment]);

        if (!response.ok)
        {
            _logger.LogError($"Message sending failed. error: {response.error}");
        }
    }

}
