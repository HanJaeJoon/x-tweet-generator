namespace TweetGenerator.Models;

public record Config
{
    public required string AzureWebJobsStorage { get; set; }
    public required string FunctionsWorkerRuntime { get; set; }
    public required string XConsumerKey { get; set; }
    public required string XConsumerKeySecret { get; set; }
    public required string XAccessKey { get; set; }
    public required string XAccessKeySecret { get; set; }
    public required string XBearerToken { get; set; }
    public required string OpenAiApiKey { get; set; }
    public required string SlackToken { get; set; }
    public List<StockConfig> Stocks { get; set; } = [];
}

public class StockConfig
{
    public required string Symbol { get; set; }
    public required string SlackChannel { get; set; }
}
