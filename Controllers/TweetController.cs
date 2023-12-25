using Microsoft.AspNetCore.Mvc;
using XGenerator.Services;

namespace XGenerator.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TweetController(TweetService tweetService)
{
    [HttpPost("")]
    public async Task PostTweet([FromBody] string tweet)
        => await tweetService.PostTweet(tweet);
}
