using System;

namespace Cutly.Models
{
    public class UrlMapping
    {
        public int Id { get; set; }
        public string OriginalUrl { get; set; } = string.Empty;
        public string ShortCode { get; set; } = string.Empty;
        public int ClickCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
} 