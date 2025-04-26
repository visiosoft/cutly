using Microsoft.EntityFrameworkCore;
using Cutly.Models;

namespace Cutly.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UrlMapping> UrlMappings { get; set; }
    }
} 