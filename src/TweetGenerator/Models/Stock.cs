namespace TweetGenerator.Models;

public class Stock
{
    public required string Symbol { get; init; }
    public required string SlackChannel { get; init; }
}
