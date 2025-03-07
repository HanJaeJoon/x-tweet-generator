using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TweetGenerator.Tests;

public static class TestStartup
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });

        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddJsonFile("test.settings.json", false, true)
            .Build());
    }
}
