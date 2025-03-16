using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Text.Json;
using TweetGenerator.Models;
using TweetGenerator.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services
            .AddTransient<YahooFinanceService>()
            .AddTransient<OpenAIService>()
            .AddTransient<TweetService>()
            .AddTransient<SlackService>()
        ;
    })
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
              .AddEnvironmentVariables();
        var builtConfig = config.Build();
    })
    .ConfigureServices((context, services) =>
    {
        var stocksString = context.Configuration["Stocks"] ?? throw new InvalidOperationException();
        var stocks = JsonSerializer.Deserialize<List<StockConfig>>(stocksString) ?? [];
        services.Configure<Config>(context.Configuration.GetSection("Values"));
        services.AddSingleton(provider =>
        {
            var config = provider.GetRequiredService<IOptions<Config>>().Value;
            config.Stocks = stocks;
            return provider.GetRequiredService<IOptions<Config>>().Value;
        });
    })
.Build();

host.Run();
