using Microsoft.Extensions.Configuration;

namespace TweetGenerator.Tests.Fixtures;

public class ConfigurationFixture
{
    public IConfiguration Configuration { get; }

    public ConfigurationFixture()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json")
            .Build();
    }
}