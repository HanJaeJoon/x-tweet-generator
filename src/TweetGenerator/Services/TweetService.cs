using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tweetinvi;
using Tweetinvi.Models;

namespace TweetGenerator.Services;

public class TweetService(IConfiguration configuration)
{
    private readonly string _consumerKey = configuration["XConsumerKey"] ?? throw new InvalidOperationException();
    private readonly string _consumerKeySecret = configuration["XConsumerKeySecret"] ?? throw new InvalidOperationException();
    private readonly string _accessKey = configuration["XAccessKey"] ?? throw new InvalidOperationException();
    private readonly string _accessKeySecret = configuration["XAccessKeySecret"] ?? throw new InvalidOperationException();

    public async Task PostTweet(string tweet, byte[]? image = null)
    {
        var userClient = new TwitterClient(
            _consumerKey,
            _consumerKeySecret,
            _accessKey,
            _accessKeySecret
        );

        var parameters = new TweetV2PostRequest() { Text = tweet };

        if (image?.Length > 0)
        {
            var uploadedImage = await userClient.Upload.UploadTweetImageAsync(image);
            if (uploadedImage?.Id is not null)
            {
                parameters.Media = new TweetV2Attacthments { MediaIds = [uploadedImage.Id.ToString()!], };
            }
        }

        var result = await userClient.Execute.AdvanceRequestAsync(
            (ITwitterRequest request) =>
            {
                var jsonBody = JsonSerializer.Serialize(parameters);
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

    private record TweetV2PostRequest
    {
        [JsonPropertyName("text")]
        public required string Text { get; init; }

        [JsonPropertyName("media")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public TweetV2Attacthments? Media { get; set; }
    }

    private record TweetV2Attacthments
    {
        [JsonPropertyName("media_ids")]
        public required string[] MediaIds { get; init; }
    }
}
