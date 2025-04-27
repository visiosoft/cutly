using System.Threading.Tasks;

namespace Cutly.Services;

public interface IUrlShortenerClientService
{
    Task<string> ShortenUrl(string longUrl);
    Task<string> GetOriginalUrl(string shortCode);
} 