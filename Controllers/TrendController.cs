using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace TrendX.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrendController : ControllerBase
{
    private readonly string? _xBearerToken;
    private readonly string? _naverClientId;
    private readonly string? _naverClientSecret;

    public TrendController(IConfiguration configuration)
    {
        _xBearerToken = configuration["X:BearerToken"];
        _naverClientId = configuration["Naver:ClientId"];
        _naverClientSecret = configuration["Naver:ClientSecret"];
    }

    [HttpGet("x/available")]
    public async Task<string> GetAvailable()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _xBearerToken);
        var response = await httpClient.GetAsync("https://api.twitter.com/1.1/trends/available.json");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    [HttpGet("x/place/{woeid}")]
    public async Task<string> GetPlace(int woeid)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _xBearerToken);
        var response = await httpClient.GetAsync($"https://api.twitter.com/1.1/trends/place.json?id={woeid}");
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }

    [HttpGet("naver/trends")]
    public async Task<string> GetNaverTrends()
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("X-NCP-APIGW-API-KEY-ID", _naverClientId);
        httpClient.DefaultRequestHeaders.Add("X-NCP-APIGW-API-KEY", _naverClientSecret);

        var yesterday = DateTime.Today.AddDays(-1);
        var data = new
        {
            startDate = yesterday.ToString("yyyy-MM-dd"),
            endDate = yesterday.ToString("yyyy-MM-dd"),
            timeUnit = "date",
            keywordGroups = new[]
            {
                new
                {
                    groupName = "트위터",
                    keywords = new[] { "트위터", "twitter", "x" },
                },
                new
                {
                    groupName = "페이스북",
                    keywords = new[] { "페이스북", "facebook" },
                },
                new
                {
                    groupName = "인스타그램",
                    keywords = new[] { "인스타그램", "instagram" },
                },
                new
                {
                    groupName = "유튜브",
                    keywords = new[] { "유튜브", "youtube" },
                },
                new
                {
                    groupName = "틱톡",
                    keywords = new[] { "틱톡", "tiktok" },
                },
            },
            // device = "pc", // optional
            // gender = "m", // optional
            // ages = new int[] { 3, 4, 5, 6 }, // optional
        };


        var response = await httpClient.PostAsJsonAsync($"https://naveropenapi.apigw.ntruss.com/datalab/v1/search", data);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
}
