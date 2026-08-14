using Microsoft.EntityFrameworkCore;

namespace PathCache.Api.Data;

public class PathCacheDbContext(DbContextOptions<PathCacheDbContext> options) : DbContext(options)
{
    public DbSet<PathRecord> Paths => Set<PathRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PathRecord>()
            .HasIndex(p => new { p.Source, p.Target })
            .IsUnique();
    }
}
