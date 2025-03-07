using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Services;

namespace TweetGenerator.Tests.Fixtures;

public class TweetServiceFixture
{
    public ServiceProvider ServiceProvider { get; private set; }

    public TweetServiceFixture()
    {
        var services = new ServiceCollection();
        services.AddSingleton<TweetService>();

        TestStartup.ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
    }
}
