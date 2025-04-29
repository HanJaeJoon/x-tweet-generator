using Microsoft.Extensions.DependencyInjection;
using TweetGenerator.Services;

namespace TweetGenerator.Tests.Fixtures;

public class OpenAIServiceFixture
{
    public ServiceProvider ServiceProvider { get; private set; }

    public OpenAIServiceFixture()
    {
        var services = new ServiceCollection();
        services.AddSingleton<OpenAIService>();

        TestStartup.ConfigureServices(services);

        ServiceProvider = services.BuildServiceProvider();
    }
}
