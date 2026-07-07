using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.V2;

namespace TweetGenerator.Services;

public class TweetService(IConfiguration configuration)
{
    private readonly TwitterClient _client = new(
        configuration["XConsumerKey"] ?? throw new InvalidOperationException(),
        configuration["XConsumerKeySecret"] ?? throw new InvalidOperationException(),
        configuration["XAccessKey"] ?? throw new InvalidOperationException(),
        configuration["XAccessKeySecret"] ?? throw new InvalidOperationException()
    );

    public async Task<string?> PostTweet(string tweet, byte[]? image = null)
    {
        var parameters = new TweetV2PostRequest { Text = tweet };

        if (image?.Length > 0)
        {
            var uploadedImage = await _client.Upload.UploadTweetImageAsync(image);
            if (uploadedImage?.Id is not null)
            {
                parameters.Media = new TweetV2Attachments { MediaIds = [uploadedImage.Id.ToString()!], };
            }
        }

        var result = await _client.Execute.AdvanceRequestAsync(request =>
        {
            var jsonBody = JsonSerializer.Serialize(parameters);
            var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            request.Query.Url = "https://api.twitter.com/2/tweets";
            request.Query.HttpMethod = Tweetinvi.Models.HttpMethod.POST;
            request.Query.HttpContent = content;
        });

        if (!result.Response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Error when posting tweet:\n{result.Content}");
        }

        var tweetContent = JsonSerializer.Deserialize<TweetContent>(result.Response.Content);

        return tweetContent?.Data?.TweetIds?.FirstOrDefault();
    }

    // X API Free Tier에서 Read 권한 없음
    public async Task<TweetV2> GetTweetInfo(long id)
    {
        var response = await _client.TweetsV2.GetTweetAsync(id);
        return response.Tweet;
    }

    private record TweetV2PostRequest
    {
        [JsonPropertyName("text")]
        public required string Text { get; init; }

        [JsonPropertyName("media")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TweetV2Attachments? Media { get; set; }
    }

    private record TweetV2Attachments
    {
        [JsonPropertyName("media_ids")]
        public required string[] MediaIds { get; init; }
    }

    private record TweetContent
    {
        [JsonPropertyName("data")]
        public TweetData? Data { get; set; }
    }

    private record TweetData
    {
        [JsonPropertyName("edit_history_tweet_ids")]
        public string[]? TweetIds { get; set; }

        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
