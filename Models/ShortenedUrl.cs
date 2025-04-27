namespace Cutly.Models;

public class ShortenedUrl
{
    public int Id { get; set; }
    public string LongUrl { get; set; } = string.Empty;
    public string ShortUrl { get; set; } = string.Empty;
    public int ClickCount { get; set; }
    public DateTime CreatedAt { get; set; }
} 