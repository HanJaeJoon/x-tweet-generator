﻿using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
        var tweet = "이 트윗은 .NET으로 작성되었습니다.\n추석 기념 API 테스트 중입니다.";
        var userClient = new TwitterClient(_consumerKey, _consumerKeySecret, _accessKey, _accessKeySecret);

        var result = await userClient.Execute.AdvanceRequestAsync(
            (ITwitterRequest request) =>
            {
                var jsonBody = userClient.Json.Serialize(new PostTweetRequest { Text = tweet });
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

        /*
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://api.twitter.com/2/tweets/");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "OAuth",
            $"oauth_consumer_key=\"{_consumerKey}\"," +
            $"oauth_token=\"{_accessKey}\"," +
            $"oauth_signature_method=\"HMAC-SHA1\"," +
            $"oauth_timestamp=\"{new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds()}\"," +
            $"oauth_nonce=\"{Guid.NewGuid()}\"," +
            $"oauth_version=\"1.0\"," +
            $"oauth_signature=\"{_consumerKeySecret}&{_accessKeySecret}\""
        );

        var content = new StringContent($"status={tweetText}");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

        var response = await httpClient.PostAsync("update.json", content);

        if (response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine(responseBody);
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
        */
    }

    public record PostTweetRequest
    {
        [JsonProperty("text")]
        public string Text { get; init; } = string.Empty;
    }
}