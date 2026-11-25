using Microsoft.EntityFrameworkCore;
using ReviewHub.Application.Abstractions;
using ReviewHub.Application.DTOs;
using ReviewHub.Domain.Entities;

namespace ReviewHub.Application.Services;

public class AuthService(IApplicationDbContext db, IJwtTokenService jwt, IPasswordHasher hasher)
{
    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 6)
            throw new ArgumentException("Name and a password of at least 6 characters are required.");
        if (await db.Users.AnyAsync(u => u.Email == email, ct))
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new User { Name = req.Name.Trim(), Email = email, PasswordHash = hasher.Hash(req.Password) };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return new AuthResponse(jwt.CreateToken(user), user.Name, user.Email);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email, ct)
            ?? throw new UnauthorizedAccessException("Invalid email or password.");
        if (!hasher.Verify(req.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");
        return new AuthResponse(jwt.CreateToken(user), user.Name, user.Email);
    }
}
