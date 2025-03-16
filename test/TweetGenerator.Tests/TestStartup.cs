using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TweetGenerator.Models;

namespace TweetGenerator.Tests;

public static class TestStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("test.settings.json", optional: false, reloadOnChange: true)
            .Build();
        services
            .Configure<Config>(configuration)
            .AddSingleton(provider => provider.GetRequiredService<IOptions<Config>>().Value)
        ;
    }
}
