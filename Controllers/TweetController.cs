using Microsoft.AspNetCore.Mvc;
using XGenerator.Services;

namespace XGenerator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TweetController(TweetService tweetService, OpenAiService openAiService)
{
    [HttpPost("")]
    public async Task PostTweet([FromBody] string tweet)
        => await tweetService.PostTweet(tweet);

    [HttpPost("generate")]
    public async Task PostTweetWithGeneratedImage([FromBody] string prompt)
    {
        prompt = """
            Flat simple vector illustration style, vibrant colors, white background,
            Male programmer sitting in front of laptop, rejoicing in the rise of stock
        """;
        var imageByte = await openAiService.GetImage(prompt);
        await tweetService.PostTweet("DALL-E 3로 생성된 이미지 테스트", imageByte);
    }
}
