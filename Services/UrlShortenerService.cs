using System;
using System.Linq;
using System.Threading.Tasks;
using Cutly.Data;
using Cutly.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace Cutly.Services
{
    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly ApplicationDbContext _context;
        private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int ShortCodeLength = 6;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _baseUrl;

        public UrlShortenerService(ApplicationDbContext context, HttpClient httpClient, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClient;
            _configuration = configuration;
            _baseUrl = configuration["BaseUrl"] ?? "http://localhost:5269";
        }

        public async Task<string> CreateShortUrl(string originalUrl, string userId)
        {
            var shortCode = GenerateShortCode();
            
            var urlMapping = new UrlMapping
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0,
                UserId = userId
            };

            _context.UrlMappings.Add(urlMapping);
            await _context.SaveChangesAsync();

            return shortCode;
        }

        public async Task<string?> GetOriginalUrl(string shortCode)
        {
            var mapping = await _context.UrlMappings
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            return mapping?.OriginalUrl;
        }

        public async Task IncrementClickCount(string shortCode)
        {
            var mapping = await _context.UrlMappings
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (mapping != null)
            {
                mapping.ClickCount++;
                await _context.SaveChangesAsync();
            }
        }

        private string GenerateShortCode()
        {
            var random = new Random();
            var shortCode = new string(Enumerable.Repeat(Characters, ShortCodeLength)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            return shortCode;
        }

        public async Task<ShortenedUrl> ShortenUrlAsync(string longUrl, string userId)
        {
            var shortCode = await CreateShortUrl(longUrl, userId);
            return new ShortenedUrl
            {
                LongUrl = longUrl,
                ShortUrl = $"{_baseUrl}/s/{shortCode}",
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0
            };
        }

        public async Task<ShortenedUrl?> GetUrlInfoAsync(string shortCode)
        {
            var mapping = await _context.UrlMappings
                .FirstOrDefaultAsync(u => u.ShortCode == shortCode);

            if (mapping == null)
                return null;

            return new ShortenedUrl
            {
                LongUrl = mapping.OriginalUrl,
                ShortUrl = $"{_baseUrl}/s/{mapping.ShortCode}",
                CreatedAt = mapping.CreatedAt,
                ClickCount = mapping.ClickCount
            };
        }

        public async Task<IEnumerable<ShortenedUrl>> GetRecentUrlsAsync()
        {
            var mappings = await _context.UrlMappings
                .OrderByDescending(u => u.CreatedAt)
                .Take(10)
                .ToListAsync();

            return mappings.Select(m => new ShortenedUrl
            {
                LongUrl = m.OriginalUrl,
                ShortUrl = $"{_baseUrl}/s/{m.ShortCode}",
                CreatedAt = m.CreatedAt,
                ClickCount = m.ClickCount
            });
        }

        public async Task IncrementClickCountAndTrackIp(string shortCode, string ipAddress)
        {
            var mapping = await _context.UrlMappings.FirstOrDefaultAsync(u => u.ShortCode == shortCode);
            if (mapping != null)
            {
                mapping.ClickCount++;
                if (!mapping.AccessIpAddresses.Contains(ipAddress))
                {
                    mapping.AccessIpAddresses.Add(ipAddress);
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ShortenedUrl>> GetUserUrlsAsync(string userId)
        {
            var mappings = await _context.UrlMappings
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return mappings.Select(m => new ShortenedUrl
            {
                LongUrl = m.OriginalUrl,
                ShortUrl = $"{_baseUrl}/s/{m.ShortCode}",
                CreatedAt = m.CreatedAt,
                ClickCount = m.ClickCount
            });
        }

        public async Task<IEnumerable<UrlMapping>> GetUrlMappingsForUserAsync(string userId)
        {
            return await _context.UrlMappings
                .Where(u => u.UserId == userId)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
        }
    }
} 