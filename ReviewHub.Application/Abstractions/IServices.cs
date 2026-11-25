using ReviewHub.Domain.Entities;

namespace ReviewHub.Application.Abstractions;

public interface IApplicationDbContext
{
    Microsoft.EntityFrameworkCore.DbSet<User> Users { get; }
    Microsoft.EntityFrameworkCore.DbSet<Place> Places { get; }
    Microsoft.EntityFrameworkCore.DbSet<Review> Reviews { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}

public interface IJwtTokenService
{
    string CreateToken(User user);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
