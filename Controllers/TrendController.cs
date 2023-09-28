using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace TrendX.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TrendController : ControllerBase
{
    private readonly string? _bearerToken;

    public TrendController(IConfiguration configuration)
    {
        _bearerToken = configuration["X:BearerToken"];
    }

    [HttpGet("available")]
    public async Task<string> GetAvailable()
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
            var response = await httpClient.GetAsync("https://api.twitter.com/1.1/trends/available.json");
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (Exception e)
        {
            return $"[Error]\n{e.Message}";
        }
    }

    [HttpGet("place/{woeid}")]
    public async Task<string> GetPlace(int woeid)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _bearerToken);
            var response = await httpClient.GetAsync($"https://api.twitter.com/1.1/trends/place.json?id={woeid}");
            var responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (Exception e)
        {
            return $"[Error]\n{e.Message}";
        }
    }
}
