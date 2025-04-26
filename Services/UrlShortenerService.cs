using System;
using System.Linq;
using System.Threading.Tasks;
using Cutly.Data;
using Cutly.Models;
using Microsoft.EntityFrameworkCore;

namespace Cutly.Services
{
    public interface IUrlShortenerService
    {
        Task<string> CreateShortUrl(string originalUrl);
        Task<string?> GetOriginalUrl(string shortCode);
        Task IncrementClickCount(string shortCode);
    }

    public class UrlShortenerService : IUrlShortenerService
    {
        private readonly ApplicationDbContext _context;
        private const string Characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private const int ShortCodeLength = 6;

        public UrlShortenerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateShortUrl(string originalUrl)
        {
            var shortCode = GenerateShortCode();
            
            var urlMapping = new UrlMapping
            {
                OriginalUrl = originalUrl,
                ShortCode = shortCode,
                CreatedAt = DateTime.UtcNow,
                ClickCount = 0
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
    }
} 