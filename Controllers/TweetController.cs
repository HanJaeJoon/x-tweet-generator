using Microsoft.AspNetCore.Mvc;
using System.Text;
using Tweetinvi;
using Tweetinvi.Models;

namespace TrendX.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TweetController
{
    private readonly string? _consumerKey;
    private readonly string? _consumerKeySecret;
    private readonly string? _accessKey;
    private readonly string? _accessKeySecret;

    public TweetController(IConfiguration configuration)
    {
        _consumerKey = configuration["X:ConsumerKey"];
        _consumerKeySecret = configuration["X:ConsumerKeySecret"];
        _accessKey = configuration["X:AccessKey"];
        _accessKeySecret = configuration["X:AccessKeySecret"];
    }

    [HttpPost("")]
    public async Task PostTweet()
    {
        var tweet = "이 트윗은 .NET으로 작성되었습니다.";
        var userClient = new TwitterClient(_consumerKey, _consumerKeySecret, _accessKey, _accessKeySecret);

        var result = await userClient.Execute.AdvanceRequestAsync(
            (ITwitterRequest request) =>
            {
                var jsonBody = userClient.Json.Serialize(new { Text = tweet });
                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                request.Query.Url = "https://api.twitter.com/2/tweets";
                request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
                request.Query.HttpContent = content;
            }
        );

        if (!result.Response.IsSuccessStatusCode)
        {
            throw new Exception($"Error when posting tweet:\n{result.Content}");
        }
    }
}
