using Microsoft.EntityFrameworkCore;
using ReviewHub.Application.Abstractions;
using ReviewHub.Domain.Entities;

namespace ReviewHub.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Place> Places => Set<Place>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>().HasIndex(u => u.Email).IsUnique();
        b.Entity<User>().Property(u => u.Email).HasMaxLength(256);
        b.Entity<User>().Property(u => u.Name).HasMaxLength(120);

        b.Entity<Place>().Property(p => p.Name).HasMaxLength(160);
        b.Entity<Place>().Property(p => p.Kind).HasConversion<string>().HasMaxLength(20);

        b.Entity<Review>().HasIndex(r => new { r.PlaceId, r.UserId }).IsUnique();
        b.Entity<Review>().Property(r => r.Text).HasMaxLength(2000);
    }
}
