using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tweetinvi;
using Tweetinvi.Models;

namespace XGenerator.Services;

public class TweetService(Configuration configuration)
{
    public async Task PostTweet(string tweet)
    {
        var userClient = new TwitterClient(
            configuration.ConsumerKey,
            configuration.ConsumerKeySecret,
            configuration.AccessKey,
            configuration.AccessKeySecret
        );

        var result = await userClient.Execute.AdvanceRequestAsync(
            (ITwitterRequest request) =>
            {
                var jsonBody = JsonSerializer.Serialize(new TweetV2PostRequest() { Text = tweet });
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

    public record TweetV2PostRequest
    {
        [JsonPropertyName("text")]
        public required string Text { get; init; }
    }
}
