using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TweetGenerator;

public class Function(ILoggerFactory loggerFactory, IConfiguration configuration)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<Function>();

    [Function("GenerateTweet")]
    public void GenerateTweet(
#if DEBUG
        [TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo myTimer
#else
        [TimerTrigger("0 */5 * * * *")] TimerInfo myTimer
#endif
        )
    {
        _logger.LogInformation(configuration["OpenAi:ApiKey"]);
        _logger.LogInformation($"function triggered");
    }
}
