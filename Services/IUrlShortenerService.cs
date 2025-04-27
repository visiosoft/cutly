using Cutly.Models;

namespace Cutly.Services
{
    public interface IUrlShortenerService
    {
        Task<string> CreateShortUrl(string originalUrl);
        Task<string?> GetOriginalUrl(string shortCode);
        Task IncrementClickCount(string shortCode);
        Task<ShortenedUrl> ShortenUrlAsync(string longUrl);
        Task<ShortenedUrl?> GetUrlInfoAsync(string shortCode);
        Task<IEnumerable<ShortenedUrl>> GetRecentUrlsAsync();
    }
} 