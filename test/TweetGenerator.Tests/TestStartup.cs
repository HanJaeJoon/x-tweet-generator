using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace TweetGenerator.Tests;

public class TestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();
        });
    }
}
