using HomeFlow.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeFlow.Data.Settings
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BinCollection> BinCollections { get; set; } = null!;
        public DbSet<RegisteredUprn> RegisteredUprns { get; set; }
        public DbSet<LogEntry> Logs { get; set; }

    }
}