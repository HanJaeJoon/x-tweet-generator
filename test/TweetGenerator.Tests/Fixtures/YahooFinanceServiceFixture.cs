using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Services;

namespace TweetGenerator.Tests.Fixtures;

public class YahooFinanceServiceFixture
{
    public ServiceProvider ServiceProvider { get; private set; }

    public YahooFinanceServiceFixture()
    {
        var services = new ServiceCollection();
        services.AddSingleton<YahooFinanceService>();

        TestStartup.ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
    }
}
