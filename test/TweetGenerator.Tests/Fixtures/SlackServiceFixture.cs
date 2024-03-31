using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Services;

namespace TweetGenerator.Tests.Fixtures;

public class SlackServiceFixture
{
    public ServiceProvider ServiceProvider { get; private set; }

    public SlackServiceFixture()
    {
        var services = new ServiceCollection();
        services.AddSingleton<SlackService>();

        var startup = new TestStartup();
        startup.ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
    }
}
