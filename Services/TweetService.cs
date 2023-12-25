using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tweetinvi;
using Tweetinvi.Models;

namespace XGenerator.Services;

public class TweetService(XConfiguration configuration)
{
    public async Task PostTweet(string tweet, byte[]? image = null)
    {
        var userClient = new TwitterClient(
            configuration.ConsumerKey,
            configuration.ConsumerKeySecret,
            configuration.AccessKey,
            configuration.AccessKeySecret
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
        public TweetV2Attacthments? Media { get; set; }
    }

    private record TweetV2Attacthments
    {
        [JsonPropertyName("media_ids")]
        public required string[] MediaIds { get; init; }
    }
}
