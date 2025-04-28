using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Cutly.Services;
using System.ComponentModel.DataAnnotations;

namespace Cutly.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlController : ControllerBase
    {
        private readonly IUrlShortenerService _urlShortenerService;

        public UrlController(IUrlShortenerService urlShortenerService)
        {
            _urlShortenerService = urlShortenerService;
        }

        [HttpPost("shorten")]
        public async Task<IActionResult> ShortenUrl([FromBody] ShortenUrlRequest request)
        {
            if (!Uri.TryCreate(request.Url, UriKind.Absolute, out var uriResult))
            {
                return BadRequest(new { error = "Invalid URL format" });
            }
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { error = "UserId is required" });
            }
            var shortCode = await _urlShortenerService.CreateShortUrl(request.Url, request.UserId);
            var shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}";
            return Ok(new { shortUrl, originalUrl = request.Url });
        }
    }

    public class ShortenUrlRequest
    {
        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
    }
} 