using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Cutly.Services;

public class UrlShortenerClientService : IUrlShortenerClientService
{
    private readonly HttpClient _httpClient;

    public UrlShortenerClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> ShortenUrl(string longUrl)
    {
        var response = await _httpClient.PostAsJsonAsync("api/urls", new { Url = longUrl });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UrlResponse>();
        return result?.ShortUrl ?? throw new Exception("Failed to create short URL");
    }

    public async Task<string> GetOriginalUrl(string shortCode)
    {
        var response = await _httpClient.GetAsync($"api/urls/{shortCode}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<UrlResponse>();
        return result?.Url ?? throw new Exception("Failed to get original URL");
    }
}

public class UrlResponse
{
    public string Url { get; set; } = "";
    public string ShortUrl { get; set; } = "";
} 