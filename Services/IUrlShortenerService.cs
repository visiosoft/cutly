using Cutly.Models;

namespace Cutly.Services
{
    public interface IUrlShortenerService
    {
        Task<string> CreateShortUrl(string originalUrl, string userId);
        Task<string?> GetOriginalUrl(string shortCode);
        Task IncrementClickCount(string shortCode);
        Task<ShortenedUrl> ShortenUrlAsync(string longUrl, string userId);
        Task<ShortenedUrl?> GetUrlInfoAsync(string shortCode);
        Task<IEnumerable<ShortenedUrl>> GetRecentUrlsAsync();
    }
} 