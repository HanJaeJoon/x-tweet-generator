using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TweetGenerator.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services
            .AddTransient<YahooFinanceService>()
            .AddTransient<OpenAiService>()
            .AddTransient<TweetService>()
            .AddTransient<SlackService>()
        ;
    })
.Build();

host.Run();
